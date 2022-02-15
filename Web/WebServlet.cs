using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using UMC.Data;
using UMC.Net;

namespace UMC.Web
{
    /// <summary>
    /// POS处理
    /// </summary>
    public class WebServlet : UMC.Net.INetHandler
    {
        class SysSession : WebSession
        {
            static string _Header;
            protected internal override void Check(WebContext context)
            {

                this.Header = _Header;
            }

            protected internal override bool IsAuthorization(string model, string command)
            {
                return true;
            }

            protected internal override IDictionary<string, object> Outer(WebClient client, WebContext context)
            {
                return null;

            }

            protected internal override void Storage(IDictionary header, WebContext context)
            {
                _Header = JSON.Serialize(header);
            }
        }
        public static List<MappingAttribute> Mappings(int category)
        {
            return WebRuntime.Categorys.FindAll(m => m.Category == category);
        }
        void Temp(UMC.Net.NetContext context)
        {
            var file = context.Url.LocalPath.Substring(5);
            string filename = UMC.Data.Reflection.ConfigPath(String.Format("Static\\TEMP\\{0}", file));
            switch (context.HttpMethod)
            {
                case "GET":
                    File(context, filename);
                    break;
                case "PUT":
                    Utility.Copy(context.InputStream, filename);
                    break;
            }
        }
        void File(UMC.Net.NetContext context, String name)
        {
            if (System.IO.File.Exists(name))
            {
                TransmitFile(context, name);
            }
            else
            {
                context.StatusCode = 404;
            }
        }
        protected void TransmitFile(UMC.Net.NetContext context, String file)
        {

            var lastIndex = file.LastIndexOf('.');

            var extName = "html";
            if (lastIndex > -1)
            {
                extName = file.Substring(lastIndex + 1);
            }
            switch (extName.ToLower())
            {
                case "pdf":
                    context.ContentType = "application/pdf";
                    break;
                case "log":
                    context.ContentType = "text/plain; charset=UTF-8";
                    break;
                case "txt":
                    context.ContentType = "text/plain";
                    break;
                case "htm":
                case "html":
                    context.ContentType = "text/html";
                    break;
                case "json":
                    context.ContentType = "text/json";
                    break;
                case "js":
                    context.ContentType = "text/javascript";
                    break;
                case "css":
                    context.ContentType = "text/css";
                    break;
                case "bmp":
                    context.ContentType = "image/bmp";
                    break;
                case "gif":
                    context.ContentType = "image/gif";
                    break;
                case "jpeg":
                case "jpg":
                    context.ContentType = "image/jpeg";
                    break;
                case "png":
                    context.ContentType = "image/png";
                    break;
                case "ico":
                    context.ContentType = "image/x-icon";
                    break;
                case "svg":
                    context.ContentType = "image/svg+xml";
                    break;
                case "mp3":
                    context.ContentType = "audio/mpeg";
                    break;
                case "mp4":
                    context.ContentType = "video/mpeg4";
                    break;
                case "xml":
                    context.ContentType = "text/xml";
                    break;
                default:
                    context.ContentType = "application/octet-stream";
                    break;
            }
            var fileInfo = new System.IO.FileInfo(file);

            context.AddHeader("Last-Modified", fileInfo.LastWriteTimeUtc.ToString("r"));
            var Since = context.Headers["If-Modified-Since"];
            if (String.IsNullOrEmpty(Since) == false)
            {
                var time = Convert.ToDateTime(Since);
                if (time >= fileInfo.LastWriteTimeUtc)
                {
                    context.StatusCode = 304;
                    return;

                }
            }
            using (System.IO.FileStream stream = System.IO.File.OpenRead(file))
            {
                stream.CopyTo(context.OutputStream);

            }

        }


        protected virtual void Domain(UMC.Net.NetContext context)
        {
            context.Redirect(WebResource.Instance().WebDomain());
        }
        void Process(UMC.Net.NetContext context)
        {
            if (String.IsNullOrEmpty(context.UserAgent))
            {
                return;
            }
            context.AddHeader("Access-Control-Allow-Origin", "*");
            context.AddHeader("Access-Control-Allow-Credentials", "true");
            context.AddHeader("Access-Control-Allow-Methods", "*");
            context.AddHeader("Access-Control-Allow-Headers", "x-requested-with,content-type");
            context.AddHeader("Cache-Control", "no-cache");

            var QueryString = new NameValueCollection(context.QueryString);
            QueryString.Add(context.Form);

            var model = QueryString["_model"];
            var cmd = QueryString["_cmd"];
            var jsonp = context.QueryString.Get("jsonp");
            if ("Upload".Equals(model) && "Command".Equals(cmd))
            {
                try
                {

                    //var wb = new Net.HttpClient();
                    var hash = UMC.Data.JSON.Deserialize<Hashtable>(new Uri(QueryString["src"]).WebRequest().Get().ReadAsString()).GetEnumerator();

                    QueryString.Clear();
                    while (hash.MoveNext())
                    {
                        QueryString.Add(hash.Key.ToString(), hash.Value.ToString());
                    }
                    model = QueryString["_model"];
                    cmd = QueryString["_cmd"];
                    if (String.IsNullOrEmpty(jsonp) == false)
                    {
                        QueryString.Add("jsonp", jsonp);
                    }
                }
                catch
                {
                    return;
                }
            }


            Process(QueryString, context.Url, context, model, cmd, url =>
            {
                context.Redirect(url.AbsoluteUri);
            });
        }

        public static void Registers(IEnumerable<System.Reflection.Assembly> assembly)
        {
            WebRuntime.Register(typeof(UMC.Data.Reflection).Assembly);
            foreach (var a in assembly)//mscorlib, 
            {
                var mpps = a.GetCustomAttributes(typeof(MappingAttribute), false);
                if (mpps.Length > 0)
                {
                    WebRuntime.Register(a);
                }
            }
        }
        void Process(NameValueCollection nvs, Uri Url, Net.NetContext context, string model, string cmd, Action<Uri> redirec)
        {
            NameValueCollection QueryString = new NameValueCollection();

            var SessionKey = String.Empty;
            var paths = new System.Collections.Generic.List<string>(Url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            if (paths.Count > 0)
                paths.RemoveAt(0);
            if (paths.Count > 0)
            {
                SessionKey = paths[0];
                paths.RemoveAt(0);
            }

            if (String.IsNullOrEmpty(model))
            {
                if (Url.Segments.Length > 4)
                {
                    if (paths.Count > 0)
                    {
                        if (paths[paths.Count - 1].IndexOf('.') > -1)
                        {
                            paths.RemoveAt(paths.Count - 1);
                        }
                    }

                    if (paths.Count > 1)
                    {
                        model = paths[0];
                        cmd = paths[1];
                    }
                    if (paths.Count > 2)
                    {
                        QueryString.Add(null, paths[2]);

                    }

                }
            }
            string start = nvs.Get("_start");
            for (var i = 0; i < nvs.Count; i++)
            {
                var key = System.Web.HttpUtility.UrlDecode(nvs.GetKey(i));
                var value = nvs.Get(i);
                if (String.IsNullOrEmpty(key))
                {
                    if (String.IsNullOrEmpty(value) == false)
                    {
                        QueryString.Add(null, value);
                    }
                }
                else if (!key.StartsWith("_"))
                {
                    QueryString.Add(key, value);
                }
            }
            var jsonp = QueryString.Get("jsonp");
            QueryString.Remove("jsonp");
            WebSession session = null;
            var writer = new System.IO.StreamWriter(context.OutputStream);

            context.ContentType = "text/javascript;charset=utf-8";
            if (String.IsNullOrEmpty(cmd) == false && cmd.EndsWith(".html"))
            {
                context.ContentType = "text/html";

            }

            if (String.IsNullOrEmpty(SessionKey) == false)
            {
                context.Cookies[UMC.Security.Membership.SessionCookieName] = SessionKey;
            }
            //try
            //{
            switch (model)
            {
                case "System":
                    switch (cmd)
                    {
                        case "Icon":
                        case "TimeSpan":
                        case "Start":
                        case "Log":
                        case "Mapping":
                            session = new SysSession();
                            break;

                        case "Setup":
                        case "Upgrade": 

                            Authorization(context); 
                            break;
                        default:
                            Authorization(context);
                            break;
                    }
                    break;
                default: 
                    Authorization(context); 
                    break;
            }
             
            var client = new WebClient(context, session);
            if (String.IsNullOrEmpty(jsonp) == false && jsonp.StartsWith("app"))
            {
                client.IsApp = true;
            }
            client.XHRTime = Utility.IntParse(context.Headers["umc-request-time"], 0) + 1;
            if (String.IsNullOrEmpty(start) == false)
            {
                client.Start(start);
            }
            else if (String.IsNullOrEmpty(model))
            {

                client.SendDialog(QueryString);
            }
            else if (String.IsNullOrEmpty(cmd))
            {
                if (model.StartsWith("[") == false)
                {
                    throw new Exception("Command is empty");
                }
            }
            else
            {
                client.Command(model, cmd, QueryString);
            }

            if (String.IsNullOrEmpty(model) == false && model.StartsWith("[") && String.IsNullOrEmpty(cmd))
            {
                client.JSONP(model, jsonp, writer);
            }
            else
            {
                if (String.IsNullOrEmpty(jsonp) == false)
                {
                    writer.Write(jsonp);
                    writer.Write('(');
                }
                client.WriteTo(writer, redirec);
                if (String.IsNullOrEmpty(jsonp) == false)
                {
                    writer.Write(")");
                }
            }
            writer.Flush();
        }
        protected virtual void Authorization(UMC.Net.NetContext context)
        {
            UMC.Net.NetContext.Authorization(context);

        }
        public static List<WebMeta> Auths()
        {
            List<WebMeta> metas = new List<WebMeta>();
            if (WebRuntime.flows.Count > 0)
            {
                var em = WebRuntime.flows.GetEnumerator();
                while (em.MoveNext())
                {

                    MappingAttribute mapping = (MappingAttribute)em.Current.Value[0].Type.GetCustomAttributes(typeof(MappingAttribute), false)[0];



                    WebAuthType authType = WebRuntime.authKeys[em.Current.Key];
                    if (authType == WebAuthType.Check || authType == WebAuthType.UserCheck)
                    {
                        metas.Add(new WebMeta().Put("key", em.Current.Key + "/*").Put("desc", mapping.Desc));


                    }


                }
            }

            if (WebRuntime.activities.Count > 0)
            {
                var em = WebRuntime.activities.GetEnumerator();
                while (em.MoveNext())
                {
                    var em3 = em.Current.Value.GetEnumerator();
                    while (em3.MoveNext())
                    {
                        MappingAttribute mapping = (MappingAttribute)em3.Current.Value.GetCustomAttributes(typeof(MappingAttribute), false)[0];
                        if (WebRuntime.authKeys.ContainsKey(em.Current.Key))
                        {
                            WebAuthType authType = WebRuntime.authKeys[em.Current.Key];
                            if (authType == WebAuthType.Check || authType == WebAuthType.UserCheck)
                            {
                                metas.Add(new WebMeta().Put("key", mapping.Model + "/" + mapping.Command).Put("desc", mapping.Desc));
                            }
                        }
                    }



                }
            }
            return metas;

        }

        class RKey
        {
            public string Name
            {
                get; set;
            }
            public string ResourceJS
            { get; set; }
            public Type Type { get; set; }
            public int PageIndex { get; set; }
        }


        static List<RKey> resourceKeys = new List<RKey>();

        static List<RKey> ResourceKeys
        {
            get
            {
                if (resourceKeys.Count == 0)
                {
                    var Initializers = new List<Data.Sql.Initializer>(Data.Sql.Initializer.Initializers());
                    foreach (var init in Data.Sql.Initializer.Initializers())
                    {
                        resourceKeys.Add(new RKey
                        {
                            Name = init.Name,
                            Type = init.GetType(),
                            ResourceJS = init.ResourceJS,
                            PageIndex = init.PageIndex,
                        });

                    }
                    resourceKeys.Sort((x, y) =>
                    {
                        return y.PageIndex.CompareTo(x.PageIndex);
                    });
                }
                return resourceKeys;
            }

        }
        static RKey GetResourceKey(string name)
        {
            return ResourceKeys.Find(e => e.Name == name);

        }
        protected bool Resource(UMC.Net.NetContext context, string path)
        {
            foreach (var key in ResourceKeys)
            {
                var initer = Reflection.CreateInstance(key.Type) as Data.Sql.Initializer;
                if (initer.Resource(context, path))
                {
                    return true;
                }
            }
            return false;
        }
        bool CheckUMC(UMC.Net.NetContext context)
        {
            var query = context.Url.Query;
            if (Utility.IsApp(context.UserAgent) || query.Contains("?jsonp=") || query.Contains("&jsonp=")
                  || query.Contains("?_model=")
                  || query.Contains("&_model="))
            {
                return true;
            }
            return false;

        }
        protected virtual bool CheckStaticFile()
        {
            return true;
        }
        #region IMIMEHandler Members

        public virtual void ProcessRequest(UMC.Net.NetContext context)
        {

            var path = context.Url.LocalPath;

            var staticFile = Data.Reflection.ConfigPath("Static" + path);

            var paths = new List<string>(context.Url.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            if (paths.Count == 0)
            {
                paths.Add("index");
                staticFile += "index.html";
            }

            if (CheckStaticFile())
            {
                switch (context.HttpMethod)
                {
                    case "GET":
                        if (System.IO.File.Exists(staticFile))
                        {
                            TransmitFile(context, staticFile);
                            return;
                        }
                        break;
                }
            }

            var lastPath = paths[paths.Count - 1];

            switch (paths[0])
            {
                case "Click":
                case "Page":
                    IndexResource(context, "Page.js", "page");
                    return;
                case "TEMP":
                    Temp(context);
                    return;
                case "js":
                    if (lastPath.EndsWith(".js"))
                    {
                        context.ContentType = "text/javascript";
                        var key = lastPath.Substring(0, lastPath.LastIndexOf('.'));

                        switch (key)
                        {
                            case "Page":
                                key = "UMC";
                                break;
                            case "UMC.Conf":
                                {
                                    var ks = ResourceKeys;
                                    for (var i = ks.Count - 1; i > -1; i--)
                                    {
                                        var initer = Reflection.CreateInstance(ks[i].Type) as Data.Sql.Initializer;
                                        if (String.IsNullOrEmpty(initer.ResourceJS) == false)
                                        {

                                            using (System.IO.Stream stream = initer.GetType().GetProperty("ResourceJS").DeclaringType.Assembly
                                                               .GetManifestResourceStream(initer.ResourceJS))
                                            {
                                                context.Output.WriteLine("/****{0} Conf****/", initer.Name);
                                                context.Output.WriteLine(new System.IO.StreamReader(stream).ReadToEnd());

                                            }

                                        }
                                    }
                                }
                                return;
                        }
                        var rKey = GetResourceKey(key);
                        if (rKey != null)
                        {
                            var initer = Reflection.CreateInstance(rKey.Type) as Data.Sql.Initializer;
                            initer.Resource(context, path);
                            return;
                        }
                    }
                    break;
                case "UMC":
                    if (paths.Count == 1)
                    {
                        IndexResource(context, "index");
                        return;
                    }
                    break;
                default:
                    {
                        if (CheckUMC(context))
                        {
                            Process(context);
                            return;
                        }
                        if (lastPath.IndexOf('.') > 0)
                        {
                            var names = lastPath.Split('.');
                            var configs = UMC.Data.Reflection.Configuration(names[0]);
                            if (configs != null)
                            {

                                UMC.Net.INetHandler handler = UMC.Data.Reflection.CreateObject(configs, names[1])
                                           as
                                UMC.Net.INetHandler;
                                if (handler != null)
                                {
                                    handler.ProcessRequest(context);
                                    return;

                                }
                            }

                        }
                        switch (context.HttpMethod)
                        {
                            case "GET":
                                var file = Data.Reflection.ConfigPath("Static/" + String.Join("/", paths.ToArray()) + ".html");
                                if (System.IO.File.Exists(file))
                                {
                                    File(context, file);
                                    return;
                                }
                                if (paths.Count < 4)
                                {
                                    var database = Reflection.Configuration("database") ?? new UMC.Data.ProviderConfiguration();
                                    foreach (var key in ResourceKeys)
                                    {
                                        var initer = Reflection.CreateInstance(key.Type) as Data.Sql.Initializer;
                                        if (database.Providers.ContainsKey(initer.ProviderName))
                                        {
                                            if (initer.Resource(context, path))
                                            {
                                                return;
                                            }
                                        }
                                    }
                                    if (paths.Count < 2)
                                    {
                                        IndexResource(context, "index");
                                        return;
                                    }
                                }
                                break;
                            default:
                                Process(context);
                                return;
                        }
                    }
                    break;

            }

            Process(context);


        }
        void IndexResource(Net.NetContext context, String key)
        {
            var ls = context.RawUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            var root = ls.Length == 0 ? "UMC" : ls[0];

            this.IndexResource(context, "/" + root, key);
        }
        protected void IndexResource(Net.NetContext context, String root, String key)
        {
            context.ContentType = "text/html";
            using (System.IO.Stream stream = typeof(WebServlet).Assembly
                               .GetManifestResourceStream(String.Format("UMC.Data.Resources.header.html")))
            {
                context.Output.WriteLine(new System.IO.StreamReader(stream).ReadToEnd().Trim());

            }
            if (String.IsNullOrEmpty(root))
            {
                context.Output.WriteLine("    <script src=\"/js/UMC.js\"></script>");

            }
            else if (root.EndsWith(".js"))
            {

                context.Output.WriteLine("    <script src=\"/js/{0}\"></script>", root);
            }
            else
            {
                context.Output.Write("    <script>WDK.POS.Config({posurl: ['");
                context.Output.Write(root);
                context.Output.WriteLine("/', WDK.cookie('device') || WDK.cookie('device', WDK.uuid())].join('')});");
                context.Output.WriteLine("</script>");
            }
            using (System.IO.Stream stream = typeof(WebServlet).Assembly
                               .GetManifestResourceStream(String.Format("UMC.Data.Resources.{0}.html", key)))
            {
                context.Output.WriteLine(new System.IO.StreamReader(stream).ReadToEnd());

            }
        }

        #endregion
        internal static List<WebMeta> Mapping()
        {

            List<WebMeta> metas = new List<WebMeta>();
            if (WebRuntime.webFactorys.Count > 0)
            {
                foreach (var wt in WebRuntime.webFactorys)
                {
                    var t = wt.Type;
                    WebMeta meta = new WebMeta();
                    meta.Put("type", t.FullName);
                    meta.Put("name", "." + t.Name);
                    metas.Add(meta);

                    MappingAttribute mapping = (MappingAttribute)t.GetCustomAttributes(typeof(MappingAttribute), false)[0];
                    if (String.IsNullOrEmpty(mapping.Desc) == false)
                    {
                        meta.Put("desc", mapping.Desc);

                    }

                }

            }
            if (WebRuntime.flows.Count > 0)
            {
                var em = WebRuntime.flows.GetEnumerator();
                while (em.MoveNext())
                {
                    var tls = em.Current.Value;
                    foreach (var wt in tls)
                    {
                        var t = wt.Type;
                        WebMeta meta = new WebMeta();
                        meta.Put("type", t.FullName);
                        meta.Put("name", em.Current.Key + ".");
                        meta.Put("auth", WebRuntime.authKeys[em.Current.Key].ToString().ToLower());
                        meta.Put("model", em.Current.Key);//.getKey())
                        metas.Add(meta);

                        var mappings = t.GetCustomAttributes(typeof(MappingAttribute), false);//[0];

                        MappingAttribute mapping = (MappingAttribute)mappings[0];
                        if (mappings.Length > 1)
                        {
                            foreach (var m in mappings)
                            {
                                var c = m as MappingAttribute;
                                if (String.Equals(c.Model, em.Current.Key))
                                {
                                    mapping = c;
                                    break;
                                }
                            }
                        }
                        if (String.IsNullOrEmpty(mapping.Desc) == false)
                        {
                            meta.Put("desc", mapping.Desc);

                        }

                    }


                }
            }
            if (WebRuntime.activities.Count > 0)
            {
                var em = WebRuntime.activities.GetEnumerator();
                while (em.MoveNext())
                {
                    var em3 = em.Current.Value.GetEnumerator();
                    while (em3.MoveNext())
                    {
                        var mappings = em3.Current.Value.GetCustomAttributes(typeof(MappingAttribute), false);
                        MappingAttribute mapping = (MappingAttribute)mappings[0];
                        if (mappings.Length > 1)
                        {
                            foreach (var m in mappings)
                            {
                                var c = m as MappingAttribute;
                                if (String.Equals(c.Model, em.Current.Key) && String.Equals(c.Command, em3.Current.Key))
                                {
                                    mapping = c;
                                    break;
                                }
                            }
                        }

                        WebAuthType authType = mapping.Auth;

                        WebMeta meta = new WebMeta();
                        meta.Put("type", em3.Current.Value.FullName);
                        meta.Put("name", em.Current.Key + "." + em3.Current.Key);
                        meta.Put("auth", authType.ToString().ToLower());
                        meta.Put("model", mapping.Model);//.getKey())
                        meta.Put("cmd", mapping.Command);//.getKey())
                        metas.Add(meta);

                        if (String.IsNullOrEmpty(mapping.Desc) == false)
                        {
                            meta.Put("desc", mapping.Desc);

                        }


                    }



                }
            }
            return metas;
        }

    }
}