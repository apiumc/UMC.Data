using System;
namespace UMC.Net
{

    class NetBridgeRequest : HttpMimeBody
    {
        NetBridgeClient client;
        public NetBridgeRequest(int pid, NetBridgeClient client)
        {
            this.pid = pid;
            this.client = client;
        }
        int pid;
        public static readonly byte[] X_UMC_Origin = System.Text.ASCIIEncoding.UTF8.GetBytes("\r\nX-UMC-Origin:");

      
         
        byte[] _buffers = new byte[102400];
        int bufferSize = 0;

        Uri url = null;
        int timeout = 0; 

        protected override void Header(byte[] data, int offset, int size)
        {
            int end = offset + size;
            int bridgeStart = UMC.Data.Utility.FindIndexIgnoreCase(data, offset, end, X_UMC_Origin);
            if (bridgeStart > 0)
            {
                int startIndex = bridgeStart + X_UMC_Origin.Length;
                int bridgeEnd = UMC.Data.Utility.FindIndexIgnoreCase(data, startIndex, end + HeaderEnd.Length, new byte[] { 13, 10 });


                var urlString = System.Text.Encoding.UTF8.GetString(data, startIndex + 1, bridgeEnd - startIndex - 1);
                var pathIndex = urlString.IndexOf('/', 10);

                if (pathIndex > 0)
                {
                    this.timeout = UMC.Data.Utility.IntParse(urlString.Substring(pathIndex + 1), 0);

                }
                this.url = new Uri(urlString);
                bufferSize = bridgeStart - offset;
                Array.Copy(data, offset, this._buffers, 0, bridgeStart - offset);
                Array.Copy(data, bridgeEnd, this._buffers, bufferSize, end - bridgeEnd);
                bufferSize += end - bridgeEnd; 
            }
        }
        protected override void MimeBody(byte[] data, int offset, int size)
        {
            
            if (bufferSize > 0)
            {
                if (bufferSize + size > _buffers.Length)
                {
                    byte[] headerByte = new byte[_buffers.Length + size + 1024];
                    Array.Copy(_buffers, 0, headerByte, 0, bufferSize);
                    this._buffers = headerByte;
                }
                Array.Copy(data, offset, _buffers, bufferSize, size);
                bufferSize += size;

            }
        }
        public override void Finish()
        {

            if (bufferSize > 0)
            {

                new NetBridgeResponse(this.pid, this.url, this._buffers, 0, bufferSize, this.client);
            }
            this.client = null;
            this._buffers = null;
        } 


    }
}
