using System;
using System.Collections.Concurrent;
using System.IO.Pipes;

namespace UMC.Net
{

    public class NetBridgeClient
    {
        System.IO.Stream readStream;
        System.IO.Stream writeStream;
        public NetBridgeClient(System.IO.Stream readStream, System.IO.Stream writeStream)
        {
            this.readStream = readStream;
            this.writeStream = writeStream;

            this.IsRun = true;

            this.readStream.BeginRead(this._data, 0, this._data.Length, EndRead, null);

        }

        public virtual HttpMimeBody Bridge(int pid)
        {
            return new NetBridgeRequest(pid, this);

        }
        void EndRead(IAsyncResult result)
        {
            var l = 0;
            try
            {

                l = this.readStream.EndRead(result);
            }
            catch //(Exception ex)
            {
                Close();
                return;
            }
            if (l > 0)
            {
                this.AppendData(_data, 0, l);
            }
            else
            {
                //Console.WriteLine("桥接失去连接:读起长度为0");

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
            catch //(Exception ex)
            {
                //Console.WriteLine($"桥接失去连接:{ex.Message}");

                Close();

            }
        }
        byte[] _data = new byte[10240];
        protected bool IsRun;
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
            this.writeStream.Close();
            this.writeStream.Dispose();
            if (readStream != writeStream)
            {
                readStream.Close();
                this.readStream.Dispose();
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
                            Write();

                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine($"桥接失去连接:{ex.Message}");
                            this.Close();
                        }
                    }, null);

                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"桥接失去连接:{ex.Message}");
                    this.Close();
                }
            }

        }
        public virtual void Write(int pid, byte[] data, int offset, int count)
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
        protected virtual void AppendData(HttpMimeBody mimeBody, byte[] data, int offset, int count)
        {
            mimeBody.AppendData(data, offset, count);
        }
        ConcurrentDictionary<int, HttpMimeBody> Clients = new ConcurrentDictionary<int, HttpMimeBody>();



        int curpid = 0, length = 0;
        byte[] _buffer = new byte[0];

        public void AppendData(byte[] buffer, int offset, int size)
        {
            int len = length, pid = curpid;
            if (_buffer.Length > 0)
            {
                var s = 9 - _buffer.Length;
                var buffer2 = new byte[9];
                Array.Copy(_buffer, 0, buffer2, 0, _buffer.Length);
                Array.Copy(buffer, offset, buffer2, _buffer.Length, s);

                pid = BitConverter.ToInt32(buffer2, 1);
                len = BitConverter.ToInt32(buffer2, 5);

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
                            postion += 9;
                        }
                        else
                        {
                            _buffer = new byte[size + offset - postion];

                            Array.Copy(buffer, postion, _buffer, 0, _buffer.Length);
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

                        HttpMimeBody proxy = this.Clients.GetOrAdd(pid, this.Bridge);

                        try
                        {
                            this.AppendData(proxy, buffer, index, length);
                        }
                        finally
                        {
                            if (proxy.IsMimeFinish)
                            {
                                this.Clients.TryRemove(pid, out proxy);
                            }
                        }


                    }

                }
            }
            curpid = pid;
            length = len;

        }
    }
}
