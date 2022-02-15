using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace UMC.Net
{
    /// <summary>
    /// 功能与HttpWebResponse一样，网络请求只是用
    /// </summary>

    public class NetHttpResponse : HttpMimeBody, IDisposable
    {
        HttpWebRequest webRequest;

        string m_ProtocolVersion;
        public String ProtocolVersion
        {
            get
            {
                return m_ProtocolVersion;
            }
        }
        static void Http(byte[] header, byte[] body, HttpWebRequest webRequest, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse httpResponse = new NetHttpResponse(prepare);
            httpResponse.webRequest = webRequest;

            NetProxy.Instance(webRequest.RequestUri, webRequest.ReadWriteTimeout, body.Length, tcp =>
           {
               tcp.Before(httpResponse);
               tcp.Header(header, 0, header.Length);
               if (body.Length > 0)
               {
                   tcp.Body(body, 0, body.Length);
               }
               tcp.Receive();
           }, ex =>
            {
                httpResponse.IsHttpFormatError = true;
                httpResponse.ReceiveError(ex);
            });

        }
        static void Http(byte[] header, NetContext context, HttpWebRequest webRequest, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse httpResponse = new NetHttpResponse(prepare);
            httpResponse.webRequest = webRequest;

            NetProxy.Instance(webRequest.RequestUri, webRequest.ReadWriteTimeout, context.ContentLength ?? 0, tcp =>
             {
                 tcp.Before(httpResponse);
                 tcp.Header(header, 0, header.Length);
                 // bool isError = false;
                 context.ReadAsData((b, i, c) =>
                 {
                     if (b.Length == 0 && c == 0)
                     {
                         if (i == -1)
                         {
                             tcp.Dispose();
                             httpResponse.IsHttpFormatError = true;
                             httpResponse.ReceiveError(new WebException("接收Body异常"));
                         }
                         else
                         {
                             try
                             {
                                 tcp.Receive();
                             }
                             catch (Exception ex)
                             {
                                 tcp.Dispose();
                                 httpResponse.IsHttpFormatError = true;
                                 httpResponse.ReceiveError(ex);
                             }
                         }
                     }
                     else
                     {
                         tcp.Body(b, i, c);
                     }
                 });
             }, ex =>
             {
                 httpResponse.IsHttpFormatError = true;
                 httpResponse.ReceiveError(ex);
             });

        }
        static void Http(byte[] header, Stream input, HttpWebRequest webRequest, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse httpResponse = new NetHttpResponse(prepare);
            httpResponse.webRequest = webRequest;

            NetProxy.Instance(webRequest.RequestUri, webRequest.ReadWriteTimeout, webRequest.ContentLength, tcp =>
            {
                tcp.Before(httpResponse);
                if (input != null)
                {
                    tcp.Header(header, 0, header.Length);
                    try
                    {
                        var bufferSize = 0x1000;
                        var buffer = new byte[bufferSize];
                        int i = input.Read(buffer, 0, bufferSize);
                        while (i > 0)
                        {
                            tcp.Body(buffer, 0, i);
                            i = input.Read(buffer, 0, bufferSize);
                        }
                    }
                    finally
                    {
                        input.Close();
                        input.Dispose();
                    }
                }
                else
                {
                    tcp.Header(header, 0, header.Length);
                }

                tcp.Receive();
            }, ex =>
             {
                 httpResponse.IsHttpFormatError = true;
                 httpResponse.ReceiveError(ex);
             });
        }
        static NetHttpResponse Http(byte[] header, Action<NetProxy> input, HttpWebRequest webRequest)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            bool IsTimeOut = true;
            NetHttpResponse httpResponse = new NetHttpResponse((r) =>
            {
                if (IsTimeOut)
                {
                    IsTimeOut = false;
                    mre.Set();
                }

            });
            httpResponse.webRequest = webRequest;
            NetProxy.Instance(webRequest.RequestUri, webRequest.ReadWriteTimeout, webRequest.ContentLength, tcp =>
            {
                tcp.Before(httpResponse);
                tcp.Header(header, 0, header.Length);
                if (input != null)
                {
                    input(tcp);
                }

                tcp.Receive();

            }, ex =>
            {
                httpResponse.ReceiveError(ex);
            });
            mre.WaitOne(120000);
            mre.Close();
            mre.Dispose();

            if (IsTimeOut)
            {
                IsTimeOut = false;
                httpResponse._error = new TimeoutException();
            }
            if (httpResponse._error != null)
            {
                throw httpResponse._error;
            }
            return httpResponse;
        }
        static void CheckHeader(HttpWebRequest webRequest)
        {
            if (webRequest.CookieContainer != null)
            {
                String cookie;
                if (webRequest.CookieContainer is NetCookieContainer)
                {
                    cookie = ((NetCookieContainer)webRequest.CookieContainer).GetCookieHeader(webRequest.RequestUri);
                }
                else
                {

                    cookie = webRequest.CookieContainer.GetCookieHeader(webRequest.RequestUri);
                }
                if (String.IsNullOrEmpty(cookie) == false)
                {
                    webRequest.Headers.Add(HttpRequestHeader.Cookie, cookie);
                }
            }
            if (String.IsNullOrEmpty(webRequest.Headers[HttpRequestHeader.Host]))
            {
                webRequest.Headers[HttpRequestHeader.Host] = webRequest.Host;
            }
            webRequest.Headers["Connection"] = "keep-alive";

        }
        Exception _error;

        protected override void ReceiveError(Exception ex)
        {
            _isOK = true;
            if (ex == null)
            {
                _error = new Exception("网络请求异常");
            }
            else
            {
                _error = ex;
            }
            if (this.Prepare != null)
            {
                m_StatusCode = HttpStatusCode.BadGateway;
                this.m_HttpResponseHeaders["Content-Type"] = "text/plain; charset=utf-8";

                var by = System.Text.ASCIIEncoding.UTF8.GetBytes(_error.ToString());
                this.ContentLength = by.Length;
                this._body.Enqueue(by);
                try
                {
                    this.Prepare(this);
                }
                catch (Exception ex3)
                {
                    UMC.Data.Utility.Error("HTTP", this.webRequest.RequestUri.AbsoluteUri, ex3.ToString());
                }
                finally
                {
                    this.Prepare = null;
                }

            }
            else
            {
                lock (_sysc)
                {
                    _isOK = true;
                    if (_readData != null)
                    {
                        while (this._body.Count > 0)
                        {
                            byte[] d = _body.Dequeue();
                            _readData(d, 0, d.Length);

                        }
                        _readData(new byte[0], -1, 0);
                    }
                }
            }
        }
        internal static NetHttpResponse Create(HttpWebRequest webRequest, String method)
        {
            CheckHeader(webRequest);
            var hbt = webRequest.Headers.ToByteArray();

            var bt = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));
            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            return Http(header, r => { }, webRequest);

        }
        internal static NetHttpResponse Create(HttpWebRequest webRequest, String method, string context)
        {
            CheckHeader(webRequest);
            var ms = System.Text.Encoding.UTF8.GetBytes(context);
            webRequest.ContentLength = ms.LongLength;
            var bt = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var hbt = webRequest.Headers.ToByteArray();
            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            return Http(header, tcp =>
            {
                tcp.Body(ms, 0, ms.Length);

            }, webRequest);

        }
        internal static NetHttpResponse Create(HttpWebRequest webRequest, String method, System.Net.Http.HttpContent context)
        {
            if (context.Headers != null)
            {
                if (context.Headers.ContentType != null)
                {
                    webRequest.ContentType = context.Headers.ContentType.ToString();
                }
            }
            CheckHeader(webRequest);
            webRequest.ContentLength = context.Headers.ContentLength ?? 0;
            var bt = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var hbt = webRequest.Headers.ToByteArray();
            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            return Http(header, tcp =>
            {
                var input = context.ReadAsStreamAsync().Result;
                var bufferSize = 0x1000;
                var buffer = new byte[bufferSize];
                int i = input.Read(buffer, 0, bufferSize);
                while (i > 0)
                {
                    tcp.Body(buffer, 0, i);
                    i = input.Read(buffer, 0, bufferSize);
                }
                input.Close();
                input.Dispose();
                context.Dispose();
            }, webRequest);

        }
        internal static NetHttpResponse Create(HttpWebRequest webRequest, String method, System.IO.Stream context, long contentLength)
        {

            CheckHeader(webRequest);
            if (contentLength >= 0)
            {
                webRequest.ContentLength = contentLength;
            }
            var hbt = webRequest.Headers.ToByteArray();

            var bt = Encoding.ASCII.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            return Http(header, tcp =>
            {
                var bufferSize = 0x1000;
                var buffer = new byte[bufferSize];
                int i = context.Read(buffer, 0, bufferSize);
                while (i > 0)
                {
                    tcp.Body(buffer, 0, i);
                    i = context.Read(buffer, 0, bufferSize);
                }
            }, webRequest);

        }
        internal static void Create(HttpWebRequest webRequest, String method, Action<NetHttpResponse> prepare)
        {
            CheckHeader(webRequest);
            var hbt = webRequest.Headers.ToByteArray();
            var bt = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));
            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);

            NetHttpResponse httpResponse = new NetHttpResponse(prepare);
            httpResponse.webRequest = webRequest;

            NetProxy.Instance(webRequest.RequestUri, webRequest.ReadWriteTimeout, 0, tcp =>
            {
                tcp.Before(httpResponse);
                tcp.Header(header, 0, header.Length);
                tcp.Receive();
            }, ex =>
            {
                httpResponse.IsHttpFormatError = true;
                httpResponse.ReceiveError(ex);
            });



        }
        internal static void Create(HttpWebRequest webRequest, String method, byte[] body, Action<NetHttpResponse> prepare)
        {

            CheckHeader(webRequest);
            if (body.Length > 0)
            {
                webRequest.ContentLength = body.Length;
            }//.Headers.ContentLength ?? 0;
            var bt = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var hbt = webRequest.Headers.ToByteArray();

            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            Http(header, body, webRequest, prepare);

        }
        internal static void Create(HttpWebRequest webRequest, String method, System.Net.Http.HttpContent context, Action<NetHttpResponse> prepare)
        {
            if (context.Headers != null)
            {
                if (context.Headers.ContentType != null)
                {
                    webRequest.ContentType = context.Headers.ContentType.ToString();
                }
            }
            CheckHeader(webRequest);
            webRequest.ContentLength = context.Headers.ContentLength ?? 0;
            var bt = System.Text.UTF8Encoding.UTF8.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var hbt = webRequest.Headers.ToByteArray();

            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            Http(header, context.ReadAsStreamAsync().Result, webRequest, prepare);

        }
        internal static void Create(HttpWebRequest webRequest, NetContext context, Action<NetHttpResponse> prepare)
        {

            CheckHeader(webRequest);
            if (context.ContentLength >= 0)
            {
                webRequest.ContentLength = context.ContentLength.Value;
            }
            var hbt = webRequest.Headers.ToByteArray();


            var bt = Encoding.ASCII.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", context.HttpMethod.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            Http(header, context, webRequest, prepare);

        }
        internal static void Create(HttpWebRequest webRequest, String method, System.IO.Stream context, long contentLength, Action<NetHttpResponse> prepare)
        {

            CheckHeader(webRequest);
            if (contentLength >= 0)
            {
                webRequest.ContentLength = contentLength;
            }
            var hbt = webRequest.Headers.ToByteArray();

            var bt = Encoding.ASCII.GetBytes(String.Format("{0} {1} HTTP/1.1\r\n", method.ToUpper(), webRequest.RequestUri.PathAndQuery));

            var header = new byte[bt.Length + hbt.Length];
            Array.Copy(bt, header, bt.Length);
            Array.Copy(hbt, 0, header, bt.Length, hbt.Length);
            Http(header, context, webRequest, prepare);

        }
        private NetHttpResponse(Action<NetHttpResponse> prepare)
        {
            this.Prepare = prepare;
        }

        public NameValueCollection Headers
        {
            get
            {
                return m_HttpResponseHeaders;
            }
        }
        NameValueCollection m_HttpResponseHeaders = new NameValueCollection(new UMC.Net.Comparer()); //;//new NameValueCollection();

        public string ContentEncoding
        {
            get
            {
                return this.m_HttpResponseHeaders["Content-Encoding"];
            }
        }


        HttpStatusCode m_StatusCode;
        public HttpStatusCode StatusCode
        {
            get
            {
                return this.m_StatusCode;
            }
        }
        String m_StatusDescription;
        public String StatusDescription
        {
            get
            {
                return this.m_StatusDescription;
            }
        }
        public string ContentType
        {
            get
            {
                return this.m_HttpResponseHeaders["Content-Type"];

            }
        }


        protected override void Header(byte[] data, int offset, int size)
        {
            var utf = System.Text.Encoding.UTF8;
            var start = offset;
            try
            {
                for (var ci = 0; ci < size - 2; ci++)
                {
                    var index = ci + offset;

                    if (data[index] == 10 && data[index - 1] == 13)
                    {
                        var heaerValue = utf.GetString(data, start, index - start - 1);
                        if (start == offset)
                        {

                            var l = heaerValue.IndexOf(' ');
                            if (l > 0 && heaerValue.StartsWith("HTTP/"))
                            {
                                this.m_ProtocolVersion = heaerValue.Substring(0, l);
                                heaerValue = heaerValue.Substring(l + 1);
                                var fhv = heaerValue.IndexOf(' ');
                                if (fhv > 0)
                                {

                                    this.m_StatusCode = UMC.Data.Utility.Parse(heaerValue.Substring(0, fhv), HttpStatusCode.Continue);
                                    this.m_StatusDescription = heaerValue.Substring(fhv + 1);

                                }
                                else
                                {
                                    this.m_StatusCode = UMC.Data.Utility.Parse(heaerValue, HttpStatusCode.Continue);
                                }
                            }
                            else
                            {
                                this.IsHttpFormatError = true;
                                this.ReceiveError(new FormatException("Http格式异常"));
                                return;
                            }
                        }
                        else
                        {
                            var vi = heaerValue.IndexOf(':');
                            var key = heaerValue.Substring(0, vi);
                            var value = heaerValue.Substring(vi + 2);
                            switch (key.ToLower())
                            {
                                case "set-cookie":
                                    if (webRequest.CookieContainer != null)
                                    {
                                        if (webRequest.CookieContainer is NetCookieContainer)
                                        {
                                            NetCookieContainer netCookieContainer = (NetCookieContainer)webRequest.CookieContainer;
                                            netCookieContainer.SetCookies(webRequest.RequestUri, value);
                                        }
                                        else
                                        {
                                            webRequest.CookieContainer.SetCookies(webRequest.RequestUri, value);
                                        }
                                    }

                                    break;
                            }
                            this.m_HttpResponseHeaders.Add(key, value);

                        }

                        start = index + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                this.IsHttpFormatError = true;
                this.ReceiveError(ex);
                return;
            }
            try
            {
                Prepare(this);
                Prepare = null;
            }
            catch (Exception ex)
            {
                Prepare = null;
                this.IsHttpFormatError = true;
                this.ReceiveError(ex);
            }
        }
        Action<NetHttpResponse> Prepare;
        public bool IsReadBody
        {
            get;
            private set;
        }
        public override void Finish()
        {
            lock (_sysc)
            {
                _isOK = true;
                if (_readData != null)
                {
                    while (this._body.Count > 0)
                    {
                        byte[] d = _body.Dequeue();
                        _readData(d, 0, d.Length);

                    }
                    _readData(new byte[0], 0, 0);
                }
            }


        }
        bool _isOK;

        Queue<byte[]> _body = new Queue<byte[]>();
        NetReadData _readData;
        //int ctime = 0;
        protected override void Body(byte[] data, int offset, int size)
        {
            if (size > 0 && IsHttpFormatError == false)
            {
                if (_readData != null)
                {

                    while (this._body.Count > 0)
                    {
                        byte[] d = _body.Dequeue();
                        _readData(d, 0, d.Length);

                    }

                    _readData(data, offset, size);
                }
                else
                {

                    var d = new byte[size];
                    Array.Copy(data, offset, d, 0, size);
                    _body.Enqueue(d);
                }

            }
        }
        public void ReadAsStream(Action<System.IO.Stream> action, Action<Exception> error)
        {
            var ms = new System.IO.MemoryStream();

            this.ReadAsData((b, i, c) =>
            {
                if (c == 0 && b.Length == 0)
                {
                    if (i == -1)
                    {
                        ms.Close();
                        ms.Dispose();
                        error(_error);
                    }
                    else
                    {
                        ms.Position = 0;
                        try
                        {
                            action(ms);
                        }
                        catch (Exception ex)
                        {
                            ms.Close();
                            ms.Dispose();
                            error(ex);
                        }
                    }
                }
                else
                {
                    ms.Write(b, i, c);
                }
            });


        }
        public Exception Error => _error;
        public void ReadAsStream(System.IO.Stream stream)
        {
            bool isTimeOut = true;
            ManualResetEvent mre = new ManualResetEvent(false);
            this.ReadAsData((b, i, c) =>
            {
                if (c == 0 && b.Length == 0)
                {
                    if (isTimeOut)
                    {
                        isTimeOut = false;
                        mre.Set();
                    }
                }
                else
                {
                    stream.Write(b, i, c);
                }
            });
            mre.WaitOne(60000);
            if (isTimeOut)
            {
                isTimeOut = false;
                this._error = new TimeoutException();
            }
            mre.Close();
            mre.Dispose();
            if (this._error != null)
            {
                throw _error;
            }

        }
        Object _sysc = new object();
        public void ReadAsData(Net.NetReadData readData)
        {
            if (IsReadBody == false)
            {
                IsReadBody = true;
                lock (_sysc)
                {
                    if (this._isOK)
                    {
                        while (this._body.Count > 0)
                        {
                            byte[] d = _body.Dequeue();
                            readData(d, 0, d.Length);

                        }
                        readData(new byte[0], this.IsHttpFormatError ? -1 : 0, 0);
                    }
                    else
                    {
                        this._readData = readData;
                    }
                }

            }
            else
            {
                readData(new byte[0], this.IsHttpFormatError ? -1 : 0, 0);
            }
        }

        private bool disposedValue;
        public void Dispose()
        {
            if (!disposedValue)
            {
                _body.Clear();
                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
