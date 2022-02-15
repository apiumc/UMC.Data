using System;
namespace UMC.Net
{
    public delegate void NetReadData(byte[] buffer, int offset, int size);

    public class NetBridgeProxy : UMC.Net.NetProxy
    {
        HttpMimeBody mimeBody;

        public override void Receive()
        {

        }
        NetBridgeServer server;
        byte[] appendUrl;
        static int PID = 100000;
        String key;
        public override Net.NetProxy Create(Uri uri, int timeout)
        {
            appendUrl = System.Text.UTF8Encoding.UTF8.GetBytes($"X-UMC-Origin: {new Uri(uri, "/").AbsoluteUri}\r\n");

            this.key = uri.Authority;
            return this;

        }
        public override void Before(HttpMimeBody httpMime)
        {
            this.mimeBody = httpMime;
            httpMime.Authority = this.key;

        }
        public override void Create(Uri uri, int timeout, Action<NetProxy> action, Action<Exception> error)
        {
            this.Create(uri, timeout);
            try
            {
                action(this);
            }
            catch (Exception ex)
            {
                error(ex);
            }
        }
        public override void Header(byte[] buffer, int offset, int size)
        {
            this.pid = (System.Threading.Interlocked.Decrement(ref PID));
            if (this.pid <= 0)
            {
                PID = 100000;
            }
            this.server = NetBridgeServer.BridgeServer(this.pid, mimeBody);//  OptimalBridge();
            var length = size + appendUrl.Length;
            var data = new byte[length];

            Array.Copy(buffer, offset, data, 0, size - 2);
            Array.Copy(appendUrl, 0, data, size - 2, appendUrl.Length);
            data[data.Length - 2] = 13;
            data[data.Length - 1] = 10;
            while (true)
            {
                try
                {
                    this.server.Write(this.pid, data, 0, data.Length);
                    break;
                }
                catch (System.IO.IOException)
                {
                    NetBridgeServer.RemoveBridgeServer(this.server);
                    this.server = NetBridgeServer.BridgeServer(this.pid, mimeBody);
                }
            }

        }
        public override void Body(byte[] data, int offset, int size)
        {

            while (size > 0)
            {
                var len = size > 1014 ? 1014 : size;
                this.server.Write(this.pid, data, offset, len);

                size -= len;
                offset += len;
            }




        }
        public override void Dispose()
        {
            this.server = null;
            this.mimeBody = null;
        }
        public override void Recovery()
        {
            this.server = null;
            this.mimeBody = null;

        }

        public override bool IsNew => true;
        public override bool Active()
        {
            return false;
        }

        int pid;
    }
}
