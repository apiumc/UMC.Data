using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Drawing;
using UMC.Net;

namespace UMC.Data
{


    public class WebResource : UMC.Data.DataProvider
    {

        public const string ImageResource = "~/images/";

        public const string UserResources = "~/UserResources/";
        static WebResource _Instance;
        public static void Instance(WebResource webResource, Provider provider)
        {
            webResource.Provider = provider;
            _Instance = webResource;
        }
        public static WebResource Instance()
        {
            if (_Instance == null)
            {
                Data.Provider provider;
                var pc = Reflection.Configuration("assembly");
                if (pc != null)
                {
                    provider = pc["WebResource"] ?? Data.Provider.Create("WebResource", "UMC.Data.WebResource");
                }
                else
                {
                    provider = Data.Provider.Create("WebResource", "UMC.Data.WebResource");
                }
                _Instance = UMC.Data.Reflection.CreateObject(provider) as WebResource;
                if (_Instance == null)
                {
                    _Instance = new WebResource();
                    _Instance.Provider = provider;

                }
            }
            return _Instance;
        }

        public virtual string WebDomain()
        {
            return this.Provider["domain"] ?? "/";
        }
        public virtual string AppSecret(bool isRefresh = false)
        {
            if (isRefresh)
            {
                var skey = Utility.Guid(Guid.NewGuid());
                UMC.Data.DataFactory.Instance().Put(new Data.Entities.Config { ConfValue = skey, ConfKey = "RTL_API_LOGIN_SIGN_KEY" });
                return skey;
            }
            else
            {
                var cfg = UMC.Data.DataFactory.Instance().Config("RTL_API_LOGIN_SIGN_KEY");
                var s = String.Empty;
                if (cfg != null)
                {
                    s = cfg.ConfValue;
                }
                if (String.IsNullOrEmpty(s))
                {
                    s = Utility.Guid(Guid.NewGuid());
                    UMC.Data.DataFactory.Instance().Put(new Data.Entities.Config { ConfValue = s, ConfKey = "RTL_API_LOGIN_SIGN_KEY" });

                }
                return s;
            }

        }
        public virtual string TempDomain()
        {
            return "http://oss.365lu.cn/";

        }

        public virtual T Cache<T>(string key)
        {

            var v = UMC.Data.DataFactory.Instance().Config(key);
            if (v != null)
            {
                var v1 = UMC.Data.JSON.Deserialize<System.Collections.Hashtable>(v.ConfValue);
                var value = Activator.CreateInstance<T>();
                Data.Reflection.SetProperty(value, v1);
                return value;
            }
            return default(T);
        }
        public virtual void Cache<T>(string key, T value)
        {
            var config = new UMC.Data.Entities.Config() { ConfKey = key, ConfValue = UMC.Data.JSON.Serialize(value) };
            UMC.Data.DataFactory.Instance().Put(config);

        }
        public virtual string ImageResolve(Guid id, object seq, object size)
        {
            var kdey = "";
            switch (size.ToString())
            {
                case "0":
                    break;
                case "1":
                    kdey = "!350";
                    break;
                case "2":
                    kdey = "!200";
                    break;
                case "3":
                    kdey = "!150";
                    break;
                case "4":
                    kdey = "!100";
                    break;
                case "5":
                    kdey = "!50";
                    break;
            }
            return ResolveUrl(String.Format("{2}{0}/{1}/0.jpg{3}", id, seq, UMC.Data.WebResource.ImageResource, kdey));

        }
        public virtual string ImageResolve(Guid id, string key, int size)
        {
            var kdey = "";
            switch (size)
            {
                case 0:
                    break;
                case 1:
                    kdey = "!350";
                    break;
                case 2:
                    kdey = "!200";
                    break;
                case 3:
                    kdey = "!150";
                    break;
                case 4:
                    kdey = "!100";
                    break;
                case 5:
                    kdey = "!50";
                    break;
            }
            return ResolveUrl(String.Format("{2}{0}/{1}/0.jpg{3}", id, key, UMC.Data.WebResource.ImageResource, kdey));

        }

        public virtual void CopyResolveUrl(String source, String target)
        {
            var sourcePath = Data.Utility.MapPath(this.ResolveUrl(source));
            var targetPath = Data.Utility.MapPath(this.ResolveUrl(target));
            if (System.IO.File.Exists(sourcePath))
            {
                File.Copy(sourcePath, targetPath);
            }

        }
        public virtual void Transfer(Stream stream, string targetKey)
        {
            if (targetKey.StartsWith("bin/", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            else if (targetKey.StartsWith("app_data/", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            else if (targetKey.StartsWith("UserResources/") || targetKey.StartsWith("images/"))
            {
                Utility.Copy(stream, Reflection.ConfigPath("Static/" + targetKey));
            }
            else if (targetKey.IndexOf('.') == -1)
            {
                Utility.Copy(stream, Reflection.ConfigPath("Static/" + targetKey + ".html"));

            }
        }
        public virtual string Download(string tempKey)
        {
            return String.Format("/TEMP/{0}", tempKey.Replace("\\", "/"));

        }
        public virtual void Transfer(Uri soureUrl, string targetKey)
        {
            if (targetKey.StartsWith("bin/", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            else if (targetKey.StartsWith("app_data/", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            soureUrl.WebRequest().Get(xhr =>
            {
                xhr.ReadAsStream(stream =>
                {
                    Transfer(stream, targetKey);
                    stream.Close();
                    stream.Dispose();

                }, ex => { });

            });

        }


        public virtual string ResolveUrl(string path)
        {
            String vUrl = path;
            if (path.StartsWith("~/"))
            {
                vUrl = path.Substring(1);
            }
            else if (path.StartsWith("~"))
            {
                vUrl = "/" + path.Substring(1);
            }
            String src = this.Provider["src"];
            if (String.IsNullOrEmpty(src))
            {

                String vpath = this.Provider["appId"];

                if (String.IsNullOrEmpty(vpath) == false)
                {
                    String code = Utility.Parse36Encode(Utility.Guid(vpath).Value);
                    vpath = $"/{code}";
                }

                return String.Format("https://image.365lu.cn{0}{1}", vpath, vUrl);
            }
            return src + vUrl;
        }
        void Sign(System.Collections.Specialized.NameValueCollection nvs, System.Net.HttpWebRequest http)
        {
            var secret = this.Provider["appSecret"];
            if (String.IsNullOrEmpty(secret)==false)
            { 
                HotCache.Sign(http, nvs, secret);
            }

        }
        public virtual void Transfer(Uri uri, Guid guid, int seq, string type)
        {
            String key = String.Format("images/{0}/{1}/{2}.{3}", guid, seq, 0, type.ToLower());
            var sts = new Uri(HotCache.Uri, "Transfer").WebRequest();

            var ns = new System.Collections.Specialized.NameValueCollection();
            Sign(ns, sts);
            sts.Put(new Web.WebMeta().Put("src", uri.AbsoluteUri, "key", key), xhr => { });//.ReadAsString();

        }
        public virtual void Transfer(Uri uri, Guid guid, int seq)
        {
            Transfer(uri, guid, seq, "jpg");
        }


        public virtual void Push(Guid tid, params object[] objs)
        {
            this.Push(new Guid[] { tid }, objs);
        }
        public virtual Web.WebMeta Push(Guid tid)
        {
            String vpath = this.Provider["appId"];
            if (String.IsNullOrEmpty(vpath) == false)
            {
                var sts = new Uri(HotCache.Uri, $"Push?device={UMC.Data.Utility.Guid(tid)}").WebRequest();

                var ns = new System.Collections.Specialized.NameValueCollection();

                Sign(ns, sts);
                return JSON.Deserialize<Web.WebMeta>(sts.Get().ReadAsString());


            }
            return new Web.WebMeta();
        }

        public virtual void Push(Guid[] devices, params object[] objs)
        {
            String vpath = this.Provider["appId"];
            if (String.IsNullOrEmpty(vpath) == false && devices.Length > 0 && objs.Length > 0)
            {
                var webR = new Uri(HotCache.Uri, "Push").WebRequest();

                var ns = new System.Collections.Specialized.NameValueCollection();

                Sign(ns, webR);
                webR.Put(new Web.WebMeta().Put("device", devices).Put("data", objs), xhr => { });

            }
        }





    }
}
