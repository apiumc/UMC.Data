using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Net.Cache;

namespace UMC.Net
{

    public static class NetClient
    {
        static void Copy(System.IO.Stream d, System.IO.Stream t)
        {

            var buffer = new byte[1];
            try
            {
                while (d.Read(buffer, 0, 1) == 1)
                {
                    t.Write(buffer, 0, 1);
                }
            }
            catch
            { }
            t.Flush();
        }
        public static string GetIgnore(this System.Collections.Specialized.NameValueCollection nv, string Key)
        {
            for (var i = 0; i < nv.Count; i++)
            {
                if (String.Equals(nv.GetKey(i), Key, StringComparison.CurrentCultureIgnoreCase))
                {
                    return nv.Get(i);
                }
            }
            return null;
        }
        public static void ReadAsStream(this HttpWebResponse webResponse, System.IO.Stream writer)
        {
            // System.IO.Stream 
            var stream = webResponse.GetResponseStream();
            Copy(stream, writer);
            stream.Close();
            webResponse.Close();
        }
        public static String ReadAsString(this HttpWebResponse webResponse)
        {
            var str = new System.IO.StreamReader(Decompress(webResponse.GetResponseStream(), webResponse.ContentEncoding));
            var value = str.ReadToEnd();
            str.Close();
            webResponse.Close();
            return value;
        }
        public static String ReadAsString(this NetHttpResponse webResponse)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                webResponse.ReadAsStream(ms);
                ms.Position = 0;
                var str = new System.IO.StreamReader(Decompress(ms, webResponse.ContentEncoding));
                var value = str.ReadToEnd();
                return value;
            }
        }
        public static byte[] ReadAsByteArray(this NetHttpResponse webResponse)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                webResponse.ReadAsStream(ms);
                return ms.ToArray(); ;
            }
        }
        public static byte[] ReadAsByteArray(this HttpWebResponse webResponse)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                var stream = webResponse.GetResponseStream();
                var t = Decompress(stream, webResponse.ContentEncoding);
                Copy(t, ms);
                stream.Close();
                t.Close();
                webResponse.Close();

                return ms.ToArray(); ;
            }
        }

        static Stream Decompress(Stream response, string encoding)
        {
            switch (encoding)
            {
                case "gzip":
                    return new GZipStream(response, CompressionMode.Decompress);
                case "deflate":
                    return new DeflateStream(response, CompressionMode.Decompress);
                default:
                    return response;
            }
        }
        public static HttpWebRequest Header(this HttpWebRequest webRequest, String header)
        {
            if (String.IsNullOrEmpty(header) == false)
            {
                var hs = header.Split('\n');
                foreach (var h in hs)
                {
                    var hi = h.IndexOf(":");
                    if (hi > 0)
                    {
                        webRequest.Headers[h.Substring(0, hi).Trim()] = h.Substring(hi + 1).Trim();
                    }
                }
            }
            return webRequest;
        }
        public static NetHttpResponse Get(this HttpWebRequest webRequest)
        {
            return NetHttpResponse.Create(webRequest, "GET");
        }
        public static NetHttpResponse Delete(this HttpWebRequest webRequest)
        {
            return NetHttpResponse.Create(webRequest, "DELETE");
        }
        public static NetHttpResponse Put(this HttpWebRequest webRequest, System.Net.Http.HttpContent context)
        {
            return NetHttpResponse.Create(webRequest, "PUT", context);
        }
        public static NetHttpResponse Post(this HttpWebRequest webRequest, System.Net.Http.HttpContent context)
        {
            return NetHttpResponse.Create(webRequest, "POST", context);
        }

        public static void Get(this HttpWebRequest webRequest, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, "GET", prepare);
        }
        public static void Delete(this HttpWebRequest webRequest, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, "DELETE", prepare);
        }
        public static void Put(this HttpWebRequest webRequest, Object value, Action<NetHttpResponse> prepare)
        {
            webRequest.ContentType = "application/json; charset=UTF-8";
            var body = System.Text.Encoding.UTF8.GetBytes(UMC.Data.JSON.Serialize(value, "ts"));
            NetHttpResponse.Create(webRequest, "Put", body, prepare);
        }
        public static void Put(this HttpWebRequest webRequest, System.Net.Http.HttpContent context, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, "PUT", context, prepare);
        }
        public static void Post(this HttpWebRequest webRequest, System.Net.Http.HttpContent context, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, "POST", context, prepare);
        }
        public static void Post(this HttpWebRequest webRequest, Object value, Action<NetHttpResponse> prepare)
        {
            webRequest.ContentType = "application/json; charset=UTF-8";
            var body = System.Text.Encoding.UTF8.GetBytes(UMC.Data.JSON.Serialize(value, "ts"));
            NetHttpResponse.Create(webRequest, "POST", body, prepare);
        }
        public static NetHttpResponse Post(this HttpWebRequest webRequest, Object value)
        {
            webRequest.ContentType = "application/json; charset=UTF-8";
            var body = UMC.Data.JSON.Serialize(value, "ts");
            return NetHttpResponse.Create(webRequest, "POST", body);
        }

        public static Task<HttpWebResponse> GetAsync(this HttpWebRequest webRequest)
        {
            return SendAsync(webRequest, "GET", null);
        }
        public static Task<HttpWebResponse> DeleteAsync(this HttpWebRequest webRequest)
        {
            return SendAsync(webRequest, "DELETE", null);
        }
        public static Task<HttpWebResponse> PutAsync(this HttpWebRequest webRequest, System.Net.Http.HttpContent context)
        {
            return SendAsync(webRequest, "PUT", context);
        }
        public static Task<HttpWebResponse> PostAsync(this HttpWebRequest webRequest, System.Net.Http.HttpContent context)
        {
            return SendAsync(webRequest, "POST", context);
        }
        public static Task<HttpWebResponse> SendAsync(this HttpWebRequest webRequest, String method, System.Net.Http.HttpContent context)
        {
            webRequest.Method = method;
            webRequest.ConnectionGroupName = webRequest.RequestUri.Host;
            webRequest.KeepAlive = true;
            if (context != null)
            {
                if (context.Headers != null)
                {
                    if (context.Headers.ContentType != null)
                    {
                        webRequest.ContentType = context.Headers.ContentType.ToString();
                    }
                    if (context.Headers.ContentLength.HasValue)
                    {
                        webRequest.ContentLength = context.Headers.ContentLength ?? 0;
                    }
                }
                var stream = webRequest.GetRequestStream();
                context.ReadAsStreamAsync().Result.CopyTo(stream, 1024);

                stream.Close();
            }
            Task<HttpWebResponse> webResponse = new Task<HttpWebResponse>(() =>
             {

                 HttpWebResponse response;
                 try
                 {
                     response = (HttpWebResponse)webRequest.GetResponse();
                 }
                 catch (WebException we)
                 {
                     response = (HttpWebResponse)we.Response;
                     if (response == null)
                     {
                         throw we;
                     }
                 }
                 return response;
             });
            return webResponse;
        }

        public static NetHttpResponse Net(this HttpWebRequest webRequest, String method, string context)
        {
            return NetHttpResponse.Create(webRequest, method.ToUpper(), context);
        }
        public static NetHttpResponse Net(this HttpWebRequest webRequest, String method, System.IO.Stream context, long contextLength)
        {
            return NetHttpResponse.Create(webRequest, method.ToUpper(), context, contextLength);
        }
        public static NetHttpResponse Net(this HttpWebRequest webRequest, Net.NetContext context)
        {
            webRequest.Method = context.HttpMethod;
            switch (webRequest.Method)
            {
                case "GET":
                case "DELETE":
                    return NetHttpResponse.Create(webRequest, webRequest.Method);
                default:
                    webRequest.ContentType = context.ContentType;
                    return NetHttpResponse.Create(webRequest, webRequest.Method, context.InputStream, context.ContentLength ?? 0);

            }
        }
        public static void ReadAsString(this NetHttpResponse webResponse, Action<String> action, Action<Exception> error)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();


            webResponse.ReadAsData((b, i, c) =>
            {
                if (c == 0 && b.Length == 0)
                {
                    if (i == -1)
                    {
                        ms.Close();
                        ms.Dispose();
                        error(webResponse.Error);
                        //action(null);
                    }
                    else
                    {
                        ms.Position = 0;
                        try
                        {
                            var str = new System.IO.StreamReader(Decompress(ms, webResponse.ContentEncoding));
                            var value = str.ReadToEnd();
                            action(value);
                        }
                        catch (Exception ex)
                        {
                            error(ex);
                        }
                        finally
                        {
                            ms.Close();
                            ms.Dispose();
                        }
                    }
                }
                else
                {
                    ms.Write(b, i, c);
                }
            });

        }
        public static void ReadAsStream(this Net.NetContext context, Action<System.IO.Stream> action, Action<Exception> error)
        {
            var ms = new System.IO.MemoryStream();

            context.ReadAsData((b, i, c) =>
            {
                if (c == 0 && b.Length == 0)
                {
                    if (i == -1)
                    {
                        ms.Close();
                        ms.Dispose();
                        error(new WebException("接收Body错误"));
                        //action(null);
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
        public static void Net(this HttpWebRequest webRequest, String method, System.Net.Http.HttpContent context, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, method.ToUpper(), context, prepare);
        }
        public static void Net(this HttpWebRequest webRequest, String method, System.IO.Stream context, long contextLength, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, method.ToUpper(), context, contextLength, prepare);
        }
        public static void Net(this HttpWebRequest webRequest, String method, byte[] body, Action<NetHttpResponse> prepare)
        {
            NetHttpResponse.Create(webRequest, method.ToUpper(), body, prepare);
        }
        public static void Net(this HttpWebRequest webRequest, Net.NetContext context, Action<NetHttpResponse> prepare)
        {
            webRequest.Method = context.HttpMethod;
            switch (webRequest.Method)
            {
                case "GET":
                case "DELETE":
                    NetHttpResponse.Create(webRequest, webRequest.Method, prepare);
                    break;
                default:
                    webRequest.ContentType = context.ContentType;
                    NetHttpResponse.Create(webRequest, context, prepare);
                    break;
            }
        }
        public static HttpWebResponse Send(this HttpWebRequest webRequest, String method, System.Net.Http.HttpContent context)
        {
            webRequest.ConnectionGroupName = webRequest.RequestUri.Host;
            webRequest.KeepAlive = true;
            webRequest.Method = method;
            if (context != null)
            {
                if (context.Headers != null)
                {
                    if (context.Headers.ContentType != null)
                    {
                        webRequest.ContentType = context.Headers.ContentType.ToString();
                    }
                    if (context.Headers.ContentLength.HasValue)
                    {
                        webRequest.ContentLength = context.Headers.ContentLength ?? 0;
                    }
                }
                var stream = webRequest.GetRequestStream();

                Copy(context.ReadAsStreamAsync().Result, stream);
                stream.Close();
            }
            // webRequest.al
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException we)
            {
                response = (HttpWebResponse)we.Response;
                if (response == null)
                {
                    throw we;
                }
            }
            return response;
        }
        public static HttpWebResponse Send(this HttpWebRequest webRequest, Net.NetContext context)
        {
            webRequest.ConnectionGroupName = webRequest.RequestUri.Host;
            webRequest.KeepAlive = true;
            webRequest.Method = context.HttpMethod;
            switch (webRequest.Method)
            {
                case "GET":
                case "DELETE":
                    break;
                default:
                    if ((context.ContentLength ?? 0) > 0)
                    {
                        webRequest.ContentType = context.ContentType;
                        var stream = webRequest.GetRequestStream();

                        Copy(context.InputStream, stream);
                        stream.Close();

                    }
                    break;
            }
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException we)
            {
                response = (HttpWebResponse)we.Response;
                if (response == null)
                {
                    throw we;
                }
            }
            return response;
        }
        public static void Header(this NetHttpResponse webResponse, Net.NetContext context)
        {
            context.StatusCode = Convert.ToInt32(webResponse.StatusCode);

            var ContentType = webResponse.ContentType;

            if (String.IsNullOrEmpty(ContentType) == false)
            {
                context.ContentType = ContentType;
            }
            var headers = webResponse.Headers;
            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers.GetKey(i);
                var value = headers.Get(i);

                switch (key.ToLower())
                {
                    case "content-type":
                    case "content-length":
                    case "server":
                    case "connection":
                    case "transfer-encoding":
                        break;
                    case "set-cookie":
                        var vs = new List<String>(value.Split(';'));
                        for (var c = 0; c < vs.Count; c++)
                        {
                            var v = vs[c].TrimStart();
                            if (String.IsNullOrEmpty(v) || String.Equals("secure", v, StringComparison.CurrentCultureIgnoreCase) || v.StartsWith("domain=", StringComparison.CurrentCultureIgnoreCase))
                            {
                                vs.RemoveAt(c);
                                c--;
                            }

                        }

                        context.AddHeader(key, String.Join(";", vs.ToArray()));

                        break;

                    default:
                        context.AddHeader(key, value);
                        break;
                }
            }
            if (webResponse.ContentLength > 0)
            {
                context.ContentLength = webResponse.ContentLength;
            }
        }
        public static void Transfer(this NetHttpResponse webResponse, Net.NetContext context)
        {

            webResponse.Header(context);


            if (context.AllowSynchronousIO)
            {
                webResponse.ReadAsData((b, i, c) =>
                {
                    if (c == 0 && b.Length == 0)
                    {
                        if (i == -1)
                        {
                            context.Error(webResponse.Error);
                        }
                        else
                        {
                            context.OutputFinish();
                        }
                        //mre.Set();
                    }
                    else
                    {
                        context.OutputStream.Write(b, i, c);
                    }
                });
            }
            else
            {
                webResponse.ReadAsStream(context.OutputStream);
            }
        }
        public static void Transfer(this HttpWebResponse webResponse, Net.NetContext context)
        {

            context.StatusCode = Convert.ToInt32(webResponse.StatusCode);
            if (webResponse.ContentLength > 0)
            {
                context.ContentLength = webResponse.ContentLength;
            }
            var ContentType = webResponse.ContentType;
            var transferencoding = false;
            if (String.IsNullOrEmpty(ContentType) == false)
            {
                context.ContentType = ContentType;
            }
            var headers = webResponse.Headers;
            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers.GetKey(i);

                switch (key.ToLower())
                {
                    case "content-type":
                    case "content-length":
                    case "server":
                    case "connection":
                        break;
                    case "transfer-encoding":
                        transferencoding = true;
                        break;

                    default:
                        context.AddHeader(key, headers.Get(i));
                        break;
                }
            }
            if (webResponse.ContentLength > 0 || transferencoding)
            {
                webResponse.ReadAsStream(context.OutputStream);
            }

        }
        public static HttpWebRequest WebRequest(this Uri url, CookieContainer cookieContainer = null)
        {
            var webRequest = HttpWebRequest.CreateHttp(url);
            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            webRequest.AllowAutoRedirect = false;
            webRequest.CookieContainer = cookieContainer;
            webRequest.KeepAlive = true;
            webRequest.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36";
            return webRequest;
        }

        public static HttpWebRequest Transfer(this Net.NetContext context, Uri url, CookieContainer cookieContainer = null)
        {

            var webRequest = HttpWebRequest.CreateHttp(url);
            webRequest.AllowAutoRedirect = false;
            webRequest.CookieContainer = cookieContainer;
            webRequest.KeepAlive = true;

            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            webRequest.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            webRequest.Headers[HttpRequestHeader.Host] = webRequest.Host;


            //var acceptEncoding = "gzip, deflate";
            var Headers = context.Headers;
            var language = "zh-CN,zh;q=0.9";
            for (var i = 0; i < Headers.Count; i++)
            {
                var k = Headers.GetKey(i);
                var v = Headers.Get(i);
                switch (k.ToLower())
                {
                    case "accept-language":
                        language = v;
                        break;
                    case "accept-encoding":
                        webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
                        //acceptEncoding = v;
                        break;
                    case "content-type":
                        webRequest.ContentType = v;
                        break;
                    case "content-length":
                    case "connection":
                    case "cookie":
                    case "host":
                        break;
                    case "user-agent":
                        webRequest.UserAgent = v;
                        break;
                    //case "origin":
                    //    webRequest.Headers.Add("Origin", new Uri(url, "/").AbsoluteUri);
                    //    break;
                    //case "referer":
                    //    webRequest.Headers.Add(k, new Uri(url, v.Substring(v.IndexOf('/', 8))).AbsoluteUri);
                    //    break;
                    default:
                        webRequest.Headers.Add(k, v);
                        break;
                }
            }
            webRequest.Headers.Add("Accept-Language", language);

            return webRequest;

        }
    }

}


