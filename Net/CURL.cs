using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace UMC.Net
{
    /// <summary>
    /// 开放授权
    /// </summary>
    public class CURL : System.Collections.Specialized.NameValueCollection
    {
        public static CURL Create()
        {
            return new CURL();
        }
        class HttpFile
        {
            public byte[] Buffer
            {
                get;
                set;
            }
            public System.IO.Stream Stream { get; set; }
            public string FileName { get; set; }
            /// <summary>
            /// MIME content type of file
            /// </summary>
            public string ContentType { get; set; }
        }
        private const string _lineBreak = "\r\n";
        private const string FormBoundary = "----WebKitFormBoundaryAWkKGSOIaAH8EVRA";
        private static string GetMultipartFormContentType()
        {
            return string.Format("multipart/form-data; boundary={0}", FormBoundary);
        }
        /// <summary>
        /// 发送的编码
        /// </summary>
        protected virtual Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
        /// <summary>
        /// 开放授权
        /// </summary>
        protected CURL()
        {
            //this._WebClient = new System.Net.Http.HttpClient();
            this.PostData = new System.Collections.Specialized.NameValueCollection();
        }
        /// <summary>
        /// 发送的基本参数
        /// </summary>
        protected System.Collections.Specialized.NameValueCollection PostData
        {
            get;
            private set;
        }
        void Escape(System.IO.TextWriter writer, System.Collections.Specialized.NameValueCollection data, bool isHeader)
        {

            for (int i = 0, l = data.Count; i < l; i++)
            {
                if (isHeader == false)
                {
                    writer.Write('&');
                }
                isHeader = false;
                var value = data.Get(i);
                var key = data.GetKey(i);

                if (String.IsNullOrEmpty(key))
                {
                    if (String.IsNullOrEmpty(value) == false)
                    {
                        writer.Write(Uri.EscapeDataString(value));
                    }
                }
                else
                {
                    writer.Write(Uri.EscapeDataString(data.GetKey(i)));
                    if (value != null)
                    {
                        writer.Write('=');
                        writer.Write(Uri.EscapeDataString(value));
                    }

                }
            }
        }
        /// <summary>
        /// 添加发送数据流
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="stream">数据流</param>
        /// <param name="contentType">数据流的MIME类型</param>
        public void AddFile(string name, System.IO.Stream stream, string contentType)
        {
            if (stream == null)
            {
                throw new System.ArgumentNullException("stream");
            }
            var file = new HttpFile();
            file.Stream = stream;
            file.ContentType = contentType;
            file.FileName = name + "." + contentType.Substring(contentType.LastIndexOf('/') + 1);
            this.AddFile(name, file);
        }
        /// <summary>
        /// 添加发送数据流
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="buffer">数据流</param>
        /// <param name="contentType">数据流的MIME类型</param>
        public void AddFile(string name, byte[] buffer, string contentType)
        {

            var file = new HttpFile();
            file.Buffer = buffer;
            file.ContentType = contentType;
            file.FileName = name + "." + contentType.Substring(contentType.LastIndexOf('/') + 1);
            this.AddFile(name, file);
        }
        void AddFile(string name, HttpFile file)
        {
            this.BaseAdd(name, file);
            this.IsMultipart = true;
        }
        /// <summary>
        /// 清空发送的数据
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            this.IsMultipart = false;
        }
        string getContentType(string path)
        {
            var lastIndex = path.LastIndexOf('.');
            switch (path.Substring(lastIndex + 1).ToLower())
            {
                case "htm":
                case "html":
                    return "text/html";
                case "json":
                    return "text/json";
                case "js":
                    return "text/javascript";
                case "css":
                    return "text/css";
                case "bmp":
                    return "image/bmp";
                case "gif":
                    return "image/gif";
                case "jpeg":
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }
        /// <summary>
        /// 上传网络文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uri"></param>
        public void AddFile(string name, Uri uri)
        {

            AddFile(name, uri, null);
        }
        public void AddFile(string name, Uri uri, string contentType)
        {
            var file = new HttpFile();


            var httpContent = uri.WebRequest().Get();// this.WebClient.GetAsync(uri).Result.Content;
            file.ContentType = String.IsNullOrEmpty(contentType) ? httpContent.ContentType : contentType;//
            file.Buffer = httpContent.ReadAsByteArray();//.Result;
            if (file.Buffer == null || file.Buffer.Length == 0)
            {
                throw new ArgumentException(String.Format("{0} 内容为空", uri), "uri");
            }
            this.AddFile(name, file);
            switch (file.ContentType)
            {
                case "text/html":
                    file.FileName = name + ".html";
                    break;
                case "text/javascript":
                case "text/json":
                    file.FileName = name + ".js";
                    break;
                case "text/css":
                    file.FileName = name + ".css";
                    break;
                case "image/bmp":
                    file.FileName = name + ".bmp";
                    break;
                case "image/gif":
                    file.FileName = name + ".gif";
                    break;
                case "image/jpeg":
                    file.FileName = name + ".jpg";
                    break;
                case "image/png":
                    file.FileName = name + ".png";
                    break;
                default:
                    int t = uri.AbsolutePath.LastIndexOf('.');
                    if (t > 0)
                    {

                        file.FileName = name + uri.AbsolutePath.Substring(t);
                    }
                    else
                    {

                        file.FileName = name + ".wdk";
                    }
                    break;
            }
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void AddFile(string fileName)
        {
            var name = System.IO.Path.GetFileName(fileName);

            this.AddFile(name, fileName);
        }
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="fileName">文件名</param>
        public void AddFile(string name, string fileName)
        {
            if (!System.IO.File.Exists(fileName))
            {
                throw new System.IO.FileNotFoundException(fileName);
            }
            var file = new HttpFile();
            file.ContentType = getContentType(fileName);
            file.FileName = fileName;
            file.Stream = System.IO.File.OpenRead(fileName);


            this.AddFile(name, file);
        }
        /// <summary>
        /// 是否是以multipart/form-data格式提交数据，如果添加的上传文件，这个会自动变成true
        /// </summary>
        public bool IsMultipart
        {
            get;
            set;
        }


        void Writer(HttpFile file, System.IO.Stream stream)
        {
            if (file.Stream != null)
            {
                var dest = file.Stream;
                try
                {
                    var bytes = new byte[1024];
                    int count = dest.Read(bytes, 0, 1024);
                    while (count > 0)
                    {
                        stream.Write(bytes, 0, count);
                        count = dest.Read(bytes, 0, 1024);
                    }
                }
                finally
                {
                    dest.Close();
                    dest.Dispose();
                }
            }
            else if (file.Buffer != null)
            {
                stream.Write(file.Buffer, 0, file.Buffer.Length);
            }

        }
        /// <summary>
        /// 把配置的数据上传
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string Upload(Uri uri)
        {
            if (this.IsMultipart)
            {
                return Send(uri, "POST");
            }
            else
            {
                if (this.PostData.Count > 0)
                {
                    return Send(uri, "POST");
                }
                else
                {
                    return Send(uri, "GET");
                }
            }
        }



        public string Get(Uri uri)
        {
            return _Send(uri, "GET");
        }
        /// <summary>
        /// 把配置的数据上传
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string Post(Uri uri)
        {
            return _Send(uri, "POST");
        }
        public string UserAgent
        {
            get;
            set;
        }

        public string GetURL(Uri uri)
        {
            var sb = new StringBuilder();
            var wsb = new System.IO.StringWriter(sb);
            Escape(wsb, this.PostData, true);
            Escape(wsb, this, this.PostData.Count == 0);
            //  var url = this.GetUrl();
            if (sb.Length > 0)
            {
                if (String.IsNullOrEmpty(uri.Query))
                {
                    sb.Insert(0, '?');
                }
                else
                {
                    sb.Insert(0, '&');
                }
            }
            sb.Insert(0, uri.AbsoluteUri);
            return sb.ToString();
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <returns></returns>
        string _Send(Uri uri, string method)
        {
            if (IsMultipart)
            { method = "POST"; }
            //HttpWebRequest request = null;

            switch (method)
            {
                case "POST":

                    using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
                    {
                        var writer = new System.IO.StreamWriter(memory);
                        if (IsMultipart)
                        {
                            for (int i = 0, l = this.PostData.Count; i < l; i++)
                            {
                                var name = this.PostData.GetKey(i);
                                var value = this.PostData.Get(i);
                                writer.Write("--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}",
                                    FormBoundary, name, value, _lineBreak);
                            }
                            for (int i = 0, l = this.Count; i < l; i++)
                            {
                                var name = this.GetKey(i);
                                var value = this.BaseGet(i);
                                if (value is HttpFile)
                                {
                                    var file = value as HttpFile;
                                    var filename = file.FileName ?? name;
                                    var end = filename.LastIndexOf('\\');
                                    if (end == -1)
                                    {
                                        end = filename.LastIndexOf('/');
                                    }
                                    if (end > 0)
                                    {
                                        filename = filename.Substring(end + 1);
                                    }
                                    writer.Write("--{0}{4}Content-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"{4}Content-Type: {3}{4}{4}",
                                        FormBoundary, name, filename, file.ContentType ?? "application/octet-stream", _lineBreak);
                                    writer.Flush();
                                    this.Writer(file, writer.BaseStream);
                                    writer.Flush();
                                    writer.Write(_lineBreak);
                                }
                                else
                                {
                                    writer.Write("--{0}{3}Content-Disposition: form-data; name=\"{1}\"{3}{3}{2}{3}",
                                        FormBoundary, name, this[i], _lineBreak);
                                }
                            }
                            writer.Write("--{0}--", FormBoundary, _lineBreak);
                        }
                        else
                        {
                            Escape(writer, this.PostData, true);
                            Escape(writer, this, this.PostData.Count == 0);
                        }

                        writer.Flush();
                        memory.Position = 0;
                        var cont = new System.Net.Http.StreamContent(memory);
                        cont.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(this.IsMultipart ? GetMultipartFormContentType() : "application/x-www-form-urlencoded; charset=UTF-8");
                        //_WebClient.PostAsync()
                        var webR = uri.WebRequest();


                        if (String.IsNullOrEmpty(UserAgent) == false)
                        {
                            webR.UserAgent = UserAgent;

                        }
                        return webR.Post(cont).ReadAsString();

                    }
                default:
                    var sb = new StringBuilder();
                    var wsb = new System.IO.StringWriter(sb);
                    Escape(wsb, this.PostData, true);
                    Escape(wsb, this, this.PostData.Count == 0);
                    //  var url = this.GetUrl();
                    if (sb.Length > 0)
                    {
                        if (String.IsNullOrEmpty(uri.Query))
                        {
                            sb.Insert(0, '?');
                        }
                        else
                        {
                            sb.Insert(0, '&');
                        }
                    }
                    sb.Insert(0, uri.AbsoluteUri);
                    var webR2 = new Uri(sb.ToString()).WebRequest();
                    if (String.IsNullOrEmpty(UserAgent) == false)
                    {
                        webR2.UserAgent = UserAgent;
                    }
                    return webR2.Get().ReadAsString();

            }

        }


        protected string Send(Uri uri, string method)
        {
            return _Send(uri, method);
        }

    }
}
