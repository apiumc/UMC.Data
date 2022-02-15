using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace UMC.Net
{

    public class NetBridgeServer
    {

        System.IO.Stream readStream;
        System.IO.Stream writeStream;
        public NetBridgeServer(System.IO.Stream readStream, System.IO.Stream writeStream)
        {
            this.readStream = readStream;
            this.writeStream = writeStream;

            this.IsRun = true;
            this._data = new byte[0x1000];

            this.readStream.BeginRead(this._data, 0, this._data.Length, EndRead, null);

        }
        //protected NetBridgeServer()
        //{

        //}
        public String Key
        {
            get;
            internal set;
        }
        public Object Tag
        {
            get; set;
        }
        ConcurrentQueue<byte[]> buffers = new ConcurrentQueue<byte[]>();
        protected virtual void Write(byte[] buffer)
        {
            if (buffers.IsEmpty)
            {
                buffers.Enqueue(buffer);
                Write();
            }
            else
            {
                buffers.Enqueue(buffer);
            }
        }
        void Write()
        {
            byte[] b;
            if (buffers.TryDequeue(out b))
            {
                try
                {

                    this.writeStream.BeginWrite(b, 0, b.Length, r =>
                    {
                        try
                        {
                            this.writeStream.EndWrite(r);

                        }
                        catch (Exception ex)
                        {
                            UMC.Data.Utility.Error("BridgeServer", ex.ToString());

                            RemoveBridgeServer(this);
                            this.Close();
                        }
                        Write();
                    }, null);

                }
                catch (Exception ex)
                {
                    UMC.Data.Utility.Error("BridgeServer", ex.ToString());
                    RemoveBridgeServer(this);
                    this.Close();
                }
            }

        }

        EventHandler _CloseEvent;
        public event EventHandler CloseEvent
        {
            add
            {
                if (_CloseEvent == null)
                {
                    _CloseEvent = value;
                }
                else
                {
                    _CloseEvent += value;
                }
            }
            remove
            {
                if (_CloseEvent != null)
                {
                    _CloseEvent -= value;
                }

            }
        }
        void Close()
        {
            if (IsRun)
            {
                if (_CloseEvent != null)
                {
                    _CloseEvent(this, EventArgs.Empty);
                }
                IsRun = false;
            }
            if (this.writeStream != null)
            {
                this.writeStream.Close();
                this.writeStream.Dispose();
            }
            if (readStream != writeStream)
            {
                if (this.readStream != null)
                {
                    this.readStream.Close();
                    this.readStream.Dispose();
                }
            }
        }
        //bool isClosing
        void EndRead(IAsyncResult result)
        {
            var l = 0;
            try
            {

                l = this.readStream.EndRead(result);

            }
            catch (Exception ex)
            {
                UMC.Data.Utility.Error("BridgeServer", ex.ToString());
                RemoveBridgeServer(this);
                Close();
                return;
            }
            if (l > 0)
            {
                this.AppendData(_data, 0, l);
            }
            else
            {
                //this.Write(0, new byte[0], 0, 0);
                //Console.WriteLine("桥接读取长度为0异常");

                RemoveBridgeServer(this);
                Close();
                return;
            }
            try
            {
                if (IsRun)
                {
                    this.readStream.BeginRead(this._data, 0, this._data.Length, EndRead, null);
                }
                else
                {
                    Close();
                }

            }
            catch (Exception ex)
            {
                UMC.Data.Utility.Error("BridgeServer", ex.ToString());
                RemoveBridgeServer(this);
                Close();

            }
        }

        bool IsRun;
        byte[] _data;
        class NetBridgePool
        {

            public int Count
            {
                get
                {
                    return this.Bridges.Sum(r => r.Clients.Count);
                }
            }
            public String Key
            {
                get; set;
            }

            internal List<NetBridgeServer> Bridges = new List<NetBridgeServer>();
            internal ConcurrentDictionary<String, int> Hosts = new ConcurrentDictionary<string, int>();
            // new ConcurrentDictionary<ConcurrentDictionary<uint, NatClient> >();



        }
        static ConcurrentDictionary<String, NetBridgePool> _POOL = new ConcurrentDictionary<string, NetBridgePool>();
        //static List<NetBridgePool> _POOL = new List<NetBridgePool>();
        public static void RemoveBridgeServer(NetBridgeServer bridgeServer)
        {
            foreach (var m in bridgeServer.Clients.Values)
            {
                m.ReceiveException(null);
            }
            bridgeServer.Clients.Clear();

            NetBridgePool pool;
            if (_POOL.TryGetValue(bridgeServer.Key, out pool))
            {

                pool.Bridges.Remove(bridgeServer);
                if (pool.Bridges.Count == 0)
                {
                    _POOL.TryRemove(bridgeServer.Key, out pool);
                }
            }
        }
        public static void AppendServer(String key, NetBridgeServer bridgeServer)
        {
            bridgeServer.Key = key;
            NetBridgePool pool = _POOL.GetOrAdd(key, r => new NetBridgePool() { Key = key });

            pool.Bridges.Add(bridgeServer);

        }
        public static bool CheckBridgeServer(String key)
        {
            return _POOL.ContainsKey(key);
        }
        public static NetBridgeServer BridgeServer(String key, int pid, HttpMimeBody httpMime)
        {
            NetBridgePool bridgePool;
            if (_POOL.TryGetValue(key, out bridgePool))
            {
                var server = bridgePool.Bridges[0];
                for (var i = 0; i < bridgePool.Bridges.Count; i++)
                {
                    var p = bridgePool.Bridges[i];
                    if (p.Clients.Count < server.Clients.Count)
                    {
                        server = p;
                    }
                }
                //bridgePool.Hosts[httpMime.Authority] = 0;
                server.Clients.TryAdd(pid, httpMime);
                return server;
            }
            return null;
        }

        public static NetBridgeServer BridgeServer(int pid, HttpMimeBody httpMime)
        {
            //_POOL.r
            if (_POOL.Count == 0)
            {
                throw new ApplicationException("无可用的桥接资源");
            }


            var bests = _POOL.Values.Where(r => r.Hosts.ContainsKey(httpMime.Authority)).ToArray();


            NetBridgePool bridgePool;// = _POOL[0];
            if (bests.Length > 0)
            {
                bridgePool = bests[0];
                var bcount = bridgePool.Count;
                for (var i = 0; i < bests.Length; i++)
                {
                    var p = bests[i];
                    if (p.Count < bridgePool.Count)
                    {
                        bridgePool = p;
                    }

                }
                var total = _POOL.Values.Sum(r => r.Bridges.Count);
                if (total > 0)
                {
                    var avg = _POOL.Values.Sum(r => r.Count) / total;
                    if (avg - bridgePool.Count < 5)
                    {
                        var bridges = bridgePool.Bridges;
                        if (bridges.Count > 0)
                        {
                            var server = bridges[0];
                            for (var i = 0; i < bridges.Count; i++)
                            {
                                var p = bridges[i];
                                if (p.Clients.Count < server.Clients.Count)
                                {
                                    server = p;
                                }
                            }
                            server.Clients.TryAdd(pid, httpMime);
                            return server;
                        }
                    }
                }
            }
            var pl = _POOL.Values.Where(r => r.Bridges.Count > 0).OrderBy(r => r.Hosts.Count).ToArray();
            if (pl.Length > 0)
            {
                bridgePool = pl[0];

                for (var i = 0; i < pl.Length; i++)
                {
                    var p = pl[i];
                    if (p.Bridges.Count == 0)
                    {
                        NetBridgePool pool;
                        _POOL.TryRemove(p.Key, out pool);
                        continue;
                    }
                    if (p.Count < bridgePool.Count || bridgePool.Bridges.Count == 0)
                    {
                        bridgePool = p;
                    }

                }
                if (bridgePool.Bridges.Count > 0)
                {
                    var server = bridgePool.Bridges[0];
                    for (var i = 0; i < bridgePool.Bridges.Count; i++)
                    {
                        var p = bridgePool.Bridges[i];
                        if (p.Clients.Count < server.Clients.Count)
                        {
                            server = p;
                        }
                    }
                    bridgePool.Hosts[httpMime.Authority] = 0;
                    server.Clients.TryAdd(pid, httpMime);
                    return server;
                }
                else
                {
                    throw new ApplicationException("无可用的桥接资源");
                }
            }
            else
            {
                throw new ApplicationException("无可用的桥接资源");

            }
        }
        public const byte STX = 0x02;
        public const byte ETX = 0x03;

        public virtual void Write(int pid, byte[] data, int offset, int count)
        {
            if (count > 0)
            {
                var size = count + 10;
                var buffer = new byte[size];
                buffer[0] = NetBridgeServer.STX;
                Array.Copy(BitConverter.GetBytes(pid), 0, buffer, 1, 4);
                Array.Copy(BitConverter.GetBytes(count), 0, buffer, 5, 4);
                Array.Copy(data, offset, buffer, 9, count);
                buffer[buffer.Length - 1] = NetBridgeServer.ETX;

                this.Write(buffer);
            }

        }
        public int Count => Clients.Count;

        ConcurrentDictionary<int, HttpMimeBody> Clients = new ConcurrentDictionary<int, HttpMimeBody>();// new ConcurrentDictionary<ConcurrentDictionary<uint, NatClient> >();

        public void Remove(int pid)
        {
            HttpMimeBody httpMime;
            Clients.TryRemove(pid, out httpMime);
        }
        protected virtual void AppendData(HttpMimeBody mimeBody, byte[] data, int offset, int count)
        {
            mimeBody.AppendData(data, offset, count);
        }
        int curpid = 0, length = 0;
        byte[] _buffer = new byte[0];
        public void AppendData(byte[] buffer, int offset, int size)
        {
            //var isRemove = false;
            int len = length, pid = curpid;
            if (_buffer.Length > 0)
            {
                var s = 9 - _buffer.Length;
                var buffer2 = new byte[9];
                Array.Copy(_buffer, 0, buffer2, 0, _buffer.Length);
                Array.Copy(buffer, offset, buffer2, _buffer.Length, s);

                pid = BitConverter.ToInt32(buffer2, 1);
                len = BitConverter.ToInt32(buffer2, 5);
                //isRemove = len == 0;
                if (len == 0)
                {
                    HttpMimeBody callback;
                    if (this.Clients.TryRemove(pid, out callback))
                    {
                        callback.ReceiveException(null);
                    }
                }
                _buffer = new byte[0];
                size -= s;
                offset += s;
            }
            int postion = offset;

            while (size + offset > postion)
            {
                if (len == 0)
                {
                    if (buffer[postion] == NetBridgeServer.STX)
                    {
                        if (postion + 9 < size + offset)
                        {
                            pid = BitConverter.ToInt32(buffer, postion + 1);
                            len = BitConverter.ToInt32(buffer, postion + 5);
                            if (len == 0)
                            {
                                HttpMimeBody callback;
                                if (this.Clients.TryRemove(pid, out callback))
                                {
                                    callback.ReceiveException(null);
                                }
                            }
                            postion += 9;
                        }
                        else
                        {
                            _buffer = new byte[size + offset - postion];

                            Array.Copy(buffer, postion, _buffer, 0, _buffer.Length);
                            //postion = -1;
                            break;
                        }
                    }
                    else if (buffer[postion] == NetBridgeServer.ETX)
                    {
                        postion++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }


                if (len > 0)
                {
                    var index = postion;
                    int count = size + offset - postion;
                    if (count > 0)
                    {
                        int length = len;
                        if (count > len)
                        {
                            postion += len;
                            len = 0;
                        }
                        else
                        {
                            length = count;
                            postion += count;
                            len -= count;
                        }
                        HttpMimeBody callback;
                        if (this.Clients.TryGetValue(pid, out callback))
                        {
                            try
                            {

                                this.AppendData(callback, buffer, index, length);
                            }
                            finally
                            {
                                if (callback.IsMimeFinish)
                                {
                                    this.Clients.TryRemove(pid, out callback);
                                }
                            }
                        }
                        else
                        {

                            //Console.WriteLine($"响应格式出错{pid}");
                            break;
                        }
                    }
                }
            }
            curpid = pid;
            length = len;
        }


    }



}
