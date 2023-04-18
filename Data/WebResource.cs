using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Drawing;
using UMC.Net;
using System.Net.WebSockets;

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
                    pc.Add(provider);// provider;
                }
                _Instance = new WebResource();
                _Instance.Provider = provider;
            }
            return _Instance;
        }

        public virtual string WebDomain()
        {
            return this.Provider["domain"] ?? "localhost";
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
        public virtual string ImageResolve(Guid id, object key, object size)
        {
            var kdey = "";
            switch (size.ToString())
            {
                case "0":
                    return ResolveUrl(String.Format("{2}{0}/{1}/0.png", id, key, UMC.Data.WebResource.ImageResource, kdey));
                case "1":
                    kdey = "m350";
                    break;
                case "2":
                    kdey = "m200";
                    break;
                case "3":
                    kdey = "m150";
                    break;
                case "4":
                    kdey = "m100";
                    break;
                case "5":
                    kdey = "m50";
                    break;
            }
            return ResolveUrl(String.Format("{2}{0}/{1}/0.png?umc-image={3}", id, key, UMC.Data.WebResource.ImageResource, kdey));

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
            switch (soureUrl.Scheme)
            {
                case "file":
                    var file = soureUrl.AbsolutePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
                    if (System.IO.File.Exists(file))
                    {
                        using (var stream = System.IO.File.OpenRead(file))
                        {
                            Transfer(stream, targetKey.TrimStart('/'));
                        }
                    }
                    break;
                default:
                    soureUrl.WebRequest().Get(xhr =>
                    {
                        xhr.ReadAsStream(stream =>
                        {
                            Transfer(stream, targetKey.TrimStart('/'));
                            stream.Close();
                            stream.Dispose();

                        }, ex => { });

                    });
                    break;
            }

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
            return vUrl;
        }
        void Sign(System.Collections.Specialized.NameValueCollection nvs, System.Net.HttpWebRequest http)
        {
            var secret = this.Provider["appSecret"];
            if (String.IsNullOrEmpty(secret) == false)
            {
                APIProxy.Sign(http, nvs, secret);
            }


        }
        public virtual void Transfer(Uri uri, Guid guid, int seq, string type)
        {
            String key = String.Format("images/{0}/{1}/{2}.{3}", guid, seq, 0, type.ToLower());
            Transfer(uri, key);
        }

        public virtual void Transfer(Uri uri, Guid guid, int seq)
        {
            Transfer(uri, guid, seq, "png");
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
                var sts = new Uri(APIProxy.Uri, $"Push?device={UMC.Data.Utility.Guid(tid)}").WebRequest();

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
                var webR = new Uri(APIProxy.Uri, "Push").WebRequest();

                var ns = new System.Collections.Specialized.NameValueCollection();

                Sign(ns, webR);
                webR.Put(new Web.WebMeta().Put("device", devices).Put("data", objs), xhr => { });

            }
        }





    }
}
