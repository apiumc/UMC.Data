using System;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;

namespace UMC.Net
{

    public class NetPool
    {
        ConcurrentStack<NetProxy> _pool = new ConcurrentStack<NetProxy>();
        public ConcurrentStack<NetProxy> Pool
        {
            get
            {
                return _pool;
            }
        }
        /// <summary>
        /// 连接错误
        /// </summary>
        public int BurstError
        {
            get;
            set;
        }
        public bool IsRestart
        {
            get; set;
        }
        //public DateTime LastErrorTime
        //{
        //    get; set;
        //}
        //public DateTime RestartTime
        //{
        //    get; set;
        //}
    }

    /// <summary>
    /// HTTP请求代码与HttpClient机制增加了端口复用
    /// </summary>
    public abstract class NetProxy
    {
        /// <summary>
        /// 默认NetProxy实现
        /// </summary>
        public static Type ProxyType = typeof(NetTcp);

        /// <summary>
        ///  是否新建连接，新连接异常直接把异常返回客户端
        /// </summary>
        public abstract bool IsNew
        {
            get;
        }
        /// <summary>
        /// 确认是连接是否可用
        /// </summary>
        /// <returns></returns>
        public abstract bool Active();

        /// <summary>
        /// 注册接收HTTP的接收器
        /// </summary>
        public virtual void Before(HttpMimeBody httpMime)
        {

        }

        /// <summary>
        /// 发送HTTP头部信息
        /// </summary>
        public abstract void Header(byte[] buffer, int offset, int size);
        /// <summary>
        /// 发送HTTP正文内容
        /// </summary>
        public abstract void Body(byte[] buffer, int offset, int size);
        /// <summary>
        /// 回收端口
        /// </summary>
        public virtual void Recovery()
        {


        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {

        }

        /// <summary>
        /// 准备接收
        /// </summary>
        public abstract void Receive();

        /// <summary>
        /// 创建新连接
        /// </summary>
        public abstract NetProxy Create(Uri uri, int timeout);
        /// <summary>
        /// 准备新连接异步
        /// </summary>
        public abstract void Create(Uri uri, int timeout, Action<NetProxy> action, Action<Exception> error);
        /// <summary>
        /// 连接池
        /// </summary>
        static ConcurrentDictionary<string, NetPool> _pool = new ConcurrentDictionary<string, NetPool>();

        /// <summary>
        /// 连接池
        /// </summary>
        public static ConcurrentDictionary<string, NetPool> Pool
        {
            get
            {
                return _pool;
            }
        }

        /// <summary>
        /// 采用连接池的方式获取网络请求代理
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static NetProxy Instance(Uri uri, int timeout, long length)
        {
            var key = uri.Authority;
            if (length < 102400)
            {
                NetPool queue;
                if (_pool.TryGetValue(key, out queue))
                {
                    NetProxy tc;

                    if (queue.Pool.TryPop(out tc))
                    {
                        if (tc.Active())
                        {
                            return tc;
                        }
                    }
                    while (queue.Pool.TryPop(out tc))
                    {
                        tc.Dispose();
                    }

                }
            }
            NetProxy proxy = (NetProxy)Activator.CreateInstance(ProxyType);

            return proxy.Create(uri, timeout);//, action, error);


        }
        public static void Check()
        {

            var ps = _pool.Values.ToArray();
            foreach (var p in ps)
            {
                var ks = p.Pool.ToArray();
                foreach (var k in ks)
                {
                    k.Active();
                }
            }
        }
        /// <summary>
        /// 采用连接池的方式获取网络请求代理异步
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static void Instance(Uri uri, int timeout, long length, Action<NetProxy> action, Action<Exception> error)
        {
            var key = uri.Authority;
            try
            {
                if (length < 102400)
                {
                    NetPool queue;
                    if (_pool.TryGetValue(key, out queue))
                    {

                        NetProxy tc;

                        if (queue.Pool.TryPop(out tc))
                        {
                            if (tc.Active())
                            {
                                action(tc);
                                return;
                            }
                        }
                        if (queue.BurstError > 10)
                        {
                            if (queue.IsRestart)
                            {
                                error(new System.Net.WebException($"{key}现不能正常提供服务"));
                                return;
                            }
                            else
                            {
                                queue.IsRestart = true;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error(ex);
                return;
            }
            NetProxy proxy = (NetProxy)Activator.CreateInstance(ProxyType);
            proxy.Create(uri, timeout, action, error);


        }
    }
    /// <summary>
    /// 对NetProxy默认实现
    /// </summary>
    class NetTcp : NetProxy, IDisposable
    {
        class AsyncResult
        {
            public Action<NetProxy> action;
            public Action<Exception> error;
        }

        static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;

        }

        void Init(String key, String host, int post, bool isSsl, int timeout, AsyncResult result)
        {

            var tcp = this;
            tcp.key = key;
            tcp.client = new TcpClient();
            tcp.port = post;
            tcp.host = host;
            tcp.activeTime = DateTime.Now;
            tcp.isSsl = isSsl;
            if (timeout > 0)
            {
                tcp.client.ReceiveTimeout = timeout;
            }
            tcp._IsNew = true;

            try
            {
                tcp.client.BeginConnect(tcp.host, tcp.port, tcp.ConnectEnd, result);
            }
            catch (Exception ex)
            {

                NetPool pool = Pool.GetOrAdd(this.key, r => new NetPool());
                pool.IsRestart = false;
                pool.BurstError++;
                result.error(ex);
            }


        }
        void Init(String key, String host, int post, bool isSSL, int timeout)
        {
            var tcp = this;
            tcp.key = key;
            tcp.client = new TcpClient();
            tcp.port = post;
            tcp.host = host;
            tcp.activeTime = DateTime.Now;
            tcp.isSsl = isSSL;
            if (timeout > 0)
            {
                tcp.client.ReceiveTimeout = timeout;
            }
            tcp.client.Connect(tcp.host, tcp.port);
            tcp._IsNew = true;

            if (isSSL)
            {
                SslStream ssl = new SslStream(client.GetStream(), false, RemoteCertificateValidationCallback);
                ssl.AuthenticateAsClient(this.host, new X509CertificateCollection(), SslProtocols.Ssl3 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls, false);

                this._stream = ssl;
            }
            else
            {
                this._stream = this.client.GetStream();
            }


        }
        void ConnectEnd(IAsyncResult result)
        {
            var callback = result.AsyncState as AsyncResult;
            try
            {
                this.client.EndConnect(result);

                if (isSsl)
                {
                    SslStream ssl = new SslStream(client.GetStream(), false, RemoteCertificateValidationCallback);
                    ssl.BeginAuthenticateAsClient(this.host, new X509CertificateCollection(), SslProtocols.Ssl3 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls, false, (ar) =>
                    {
                        try
                        {
                            ssl.EndAuthenticateAsClient(ar);
                            this._stream = ssl;
                            callback.action(this);
                        }
                        catch (Exception ex)
                        {
                            NetPool pool = Pool.GetOrAdd(this.key, r => new NetPool());
                            pool.IsRestart = false;
                            pool.BurstError++;

                            callback.error(ex);

                        }
                    }, null);

                }
                else
                {
                    this._stream = this.client.GetStream();
                    callback.action(this);
                }
            }
            catch (Exception ex)
            {
                NetPool pool = Pool.GetOrAdd(this.key, r => new NetPool());

                pool.IsRestart = false;
                pool.BurstError++;

                callback.error(ex);
            }
        }
        public override NetProxy Create(Uri uri, int timeout)
        {
            Init(uri.Authority, uri.Host, uri.Port, string.Equals(uri.Scheme, "https"), timeout);
            return this;
        }
        public override void Create(Uri uri, int timeout, Action<NetProxy> action, Action<Exception> error)
        {
            Init(uri.Authority, uri.Host, uri.Port, string.Equals(uri.Scheme, "https"), timeout, new AsyncResult { action = action, error = error });

        }
        String key;
        Stream _stream;
        TcpClient client;
        public DateTime activeTime;
        int _keepAlive = 60;
        String host;
        int port;
        bool _IsNew, isSsl;

        byte[] _sendBuffers = new byte[0x200];
        int _sendBufferSize = 0;
        public override bool IsNew => _IsNew;

        bool disposed = false;
        public override void Before(HttpMimeBody httpMime)
        {
            this.mimeBody = httpMime;
        }
        bool IsError = false;
        Exception _error;
        public override void Body(byte[] buffer, int offset, int size)
        {
            if (_IsNew == false)
            {
                if (_sendBufferSize + size > _sendBuffers.Length)
                {
                    byte[] headerByte = new byte[_sendBufferSize + size + 1024];
                    Array.Copy(_sendBuffers, 0, headerByte, 0, _sendBufferSize);
                    this._sendBuffers = headerByte;
                }
                Array.Copy(buffer, offset, _sendBuffers, _sendBufferSize, size);
                _sendBufferSize += size;
            }

            try
            {
                if (IsError == false)
                    _stream.Write(buffer, offset, size);
            }
            catch (Exception ex)
            {
                IsError = true;
                _error = ex;
            }
        }
        public override void Header(byte[] buffer, int offset, int size)
        {
            _sendBufferSize = 0;
            Body(buffer, offset, size);
        }

        public override void Recovery()
        {
            if (disposed == false)
            {
                if (this.mimeBody.IsClose || this.mimeBody.IsHttpFormatError)
                {
                    NetPool pool;
                    if (Pool.TryGetValue(this.key, out pool))
                    {
                        pool.BurstError = 0;
                    }

                    this.Dispose();
                }
                else
                {
                    this._keepAlive = this.mimeBody.KeepAlive;
                    var qu = Pool.GetOrAdd(this.key, k => new NetPool());
                    qu.BurstError = 0;
                    this.activeTime = DateTime.Now;
                    qu.Pool.Push(this);
                    this.mimeBody = null;
                }
            }
        }
        public override void Dispose()
        {
            if (disposed == false)
            {
                disposed = true;
                if (_stream != null)
                {
                    _stream.Close();
                    _stream.Dispose();
                }
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
                _buffers = null;
                mimeBody = null;
                this._sendBuffers = null;
                GC.SuppressFinalize(this);
            }
        }

        ~NetTcp()
        {
            Dispose();
        }
        public override bool Active()
        {
            this._IsNew = false;
            if (disposed == false)
            {
                if (this.client.Connected == false)
                {
                    this.Dispose();
                    return false;
                }
                else if (this._keepAlive > 0)
                {
                    var now = DateTime.Now;
                    if (this.activeTime.AddSeconds(this._keepAlive) < now)
                    {
                        this.Dispose();
                        return false;
                    }
                    return true;
                }
                else
                {
                    var now = DateTime.Now;
                    if (this.activeTime.AddSeconds(60) < now)
                    {
                        this.Dispose();
                        return false;
                    }
                    return true;
                }
            }
            return false;


        }

        HttpMimeBody mimeBody;
        public override void Receive()
        {
            if (IsError)
            {
                if (this.IsNew || _sendBufferSize <= 0)
                {
                    throw _error;
                }
                Reset();
            }
            else
            {
                if (this.mimeBody == null)
                {
                    throw new ArgumentNullException("MimeBody");
                }
                try
                {
                    _stream.BeginRead(_buffers, 0, _buffers.Length, HeaderEndRead, null);
                }
                catch
                {
                    if (this.IsNew || _sendBufferSize <= 0)
                    {
                        throw;
                    }

                    Reset();

                }
            }

        }
        void Reset()
        {
            NetPool queue;
            if (Pool.TryGetValue(key, out queue))
            {
                NetProxy tc;
                while (queue.Pool.TryPop(out tc))
                {
                    tc.Dispose();
                }
            }
            this.IsError = false;
            //this._error
            this._stream.Close();
            this._stream.Dispose();
            this.client.Close();
            this.client.Dispose();
            this.Init(this.key, this.host, this.port, this.isSsl, 0, new AsyncResult
            {
                action = (tcp) =>
                {
                    _stream.Write(_sendBuffers, 0, _sendBufferSize);
                    _stream.BeginRead(_buffers, 0, _buffers.Length, HeaderEndRead, null);
                },
                error = ex =>
                {
                    var m = this.mimeBody;
                    this.Dispose();
                    m.ReceiveException(ex);
                }
            });


        }
        void HeaderEndRead(IAsyncResult result)
        {
            if (disposed)
            {
                return;
            }
            var m = this.mimeBody;
            if (m == null)
            {
                this.Dispose();
                return;
            }
            var l = 0;
            Exception _error = null;
            try
            {
                l = _stream.EndRead(result);
            }
            catch (Exception ex)
            {
                _error = ex;
            }
            if (l == 0)
            {
                if (this.IsNew == false && _sendBufferSize > 0)
                {
                    Reset();
                }
                else
                {
                    NetPool pool;
                    if (Pool.TryGetValue(this.key, out pool))
                    {
                        pool.IsRestart = false;
                        pool.BurstError++;
                    }
                    this.Dispose();
                    m.ReceiveException(_error ?? new ArgumentOutOfRangeException("请求长度不正常"));

                }
                return;
            }
            _sendBufferSize = 0;
            this.activeTime = DateTime.Now;


            try
            {
                m.AppendData(_buffers, 0, l);

            }
            catch (Exception ex)
            {

                this.Dispose();
                m.ReceiveException(ex);
                return;
            }

            if (m.IsMimeFinish)
            {
                this.Recovery();
            }
            else if (m.IsHttpFormatError)
            {
                this.Dispose();
            }
            else
            {
                try
                {
                    _stream.BeginRead(_buffers, 0, _buffers.Length, DataEndRead, null);
                }
                catch (Exception ex)
                {
                    this.Dispose();
                    m.ReceiveException(ex);

                }
            }
        }
        int ctime = 0;
        void DataEndRead(IAsyncResult result)
        {
            if (disposed)
            {
                return;
            }
            ctime++;
            var m = this.mimeBody;
            var l = 0;
            try
            {
                l = _stream.EndRead(result);
            }
            catch (Exception ex)
            {
                NetPool pool;
                if (Pool.TryGetValue(this.key, out pool))
                {
                    pool.BurstError++;
                }
                this.Dispose();
                m.ReceiveException(ex);
                return;
            }
            if (l == 0)
            {
                NetPool pool;
                if (Pool.TryGetValue(this.key, out pool))
                {
                    pool.BurstError++;
                }
                this.Dispose();
                m.ReceiveException(new Exception("接口数据长度为零"));
                return;
            }
            this.activeTime = DateTime.Now;



            m.AppendData(_buffers, 0, l);

            if (m.IsMimeFinish)
            {
                this.Recovery();
            }
            else if (m.IsHttpFormatError)
            {

                this.Dispose();
            }
            else
            {
                if (ctime > 30000)
                {
                    this.Dispose();
                    m.ReceiveException(new StackOverflowException("获取内容超过30000次"));
                    return;
                }
                if ((ctime % 1000) == 0)
                {
                    System.Threading.Tasks.Task.Factory.StartNew(DataStartRead);
                }
                else
                {
                    DataStartRead();
                }
            }
        }
        void DataStartRead()
        {
            var m = this.mimeBody;
            try
            {
                _stream.BeginRead(_buffers, 0, _buffers.Length, DataEndRead, null);

            }
            catch (Exception ex)
            {
                NetPool pool;
                if (Pool.TryGetValue(this.key, out pool))
                {
                    pool.BurstError++;
                }
                this.Dispose();
                m.ReceiveException(ex);

            }
        }


        byte[] _buffers = new byte[0x1000];


    }
}
