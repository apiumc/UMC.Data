using System;
using System.Collections.Concurrent;

namespace UMC.Net
{

    class NetBridgeResponse : HttpMimeBody
    {

        void ErrorHtml(int statuscode, String html)
        {
            var bytes = System.Text.UTF32Encoding.UTF8.GetBytes(html);
            var header = System.Text.UTF32Encoding.UTF8.GetBytes($"HTTP/1.1 {statuscode} \r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: {bytes.Length}\r\n\r\n");

            this.client.Write(this.pid, header, 0, header.Length);
            this.client.Write(this.pid, bytes, 0, bytes.Length);
        }
        NetBridgeClient client;
        public NetBridgeResponse(int pid, Uri url, byte[] reqData, int offset, int size, NetBridgeClient client)
        {
            this.pid = pid;
            this.client = client;
            NetProxy proxy = null;
            try
            {
                proxy = UMC.Net.NetProxy.Instance(url, 0, 0);// as NetTcp;
                proxy.Before(this);
                proxy.Body(reqData, offset, size);

                proxy.Receive();
            }
            catch (Exception ex)
            {
                if (proxy != null)
                {
                    proxy.Dispose();
                }
                ErrorHtml(502, ex.ToString());
            }

        }
        bool IsSend;
        protected override void Header(byte[] data, int offset, int size)
        {
            for (int i = 0; i < HTTPHeader.Length; i++)
            {
                if (data[i] != HTTPHeader[i])
                {
                    this.IsHttpFormatError = true;
                    ReceiveError(new Exception("Http格式不正确"));
                    return;
                }
            }
            IsSend = true;
            this.client.Write(this.pid, data, offset, size);
        }
        protected override void MimeBody(byte[] data, int offset, int size)
        {

            if (IsHttpFormatError == false)
            {
                while (size > 0)
                {
                    var len = size > 1014 ? 1014 : size;

                    this.client.Write(this.pid, data, offset, len);
                    size -= len;
                    offset += len;

                }
            }
        }
        protected override void ReceiveError(Exception ex)
        {
            IsHttpFormatError = true;
            if (IsSend == false)
            {
                if (ex != null)
                {
                    ErrorHtml(502, ex.ToString());
                }
                else
                {
                    ErrorHtml(502, $"请求异常{this.pid}");
                }
            }
            else
            {
                this.client.Write(this.pid, new byte[0], 0, 0);
            }
            //Console.WriteLine($"请求异常{this.pid}:{ex}");
        }

        int pid;

        public override void Finish()
        {
            //Console.WriteLine($"完成响应{this.pid}");
        }
    }
}
