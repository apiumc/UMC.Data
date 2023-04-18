using System;
using System.Reflection;
using System.Linq;
using UMC.Data;

namespace UMC.Net
{
    public class APIProxy
    {

        public static void Subscribes(String point)
        {
            var subscribe = ProviderConfiguration.GetProvider(Reflection.AppDataPath("UMC//Subscribe.xml")) ?? new ProviderConfiguration();

            if (subscribe.Count > 0)
            {
                for (var i = 0; i < subscribe.Count; i++)
                {
                    var p = subscribe[i];
                    var url = new Uri(p["url"]);
                    new NetSubscribe(point, url.Host, url.Port, WebResource.Instance().Provider["appSecret"]);
                }
            }
        }

        public static void Sign(System.Net.HttpWebRequest http, System.Collections.Specialized.NameValueCollection nvs, String secret)
        {
            var p = Assembly.GetEntryAssembly().GetCustomAttributes().First(r => r is System.Reflection.AssemblyInformationalVersionAttribute) as System.Reflection.AssemblyInformationalVersionAttribute;

            nvs.Add("umc-app-version", p.InformationalVersion);
            nvs.Add("umc-client-pfm", "sync");
            nvs.Add("umc-request-time", UMC.Data.Utility.TimeSpan().ToString());
            nvs.Add("umc-request-sign", UMC.Data.Utility.Sign(nvs, secret));


            for (var i = 0; i < nvs.Count; i++)
            {
                http.Headers.Add(nvs.GetKey(i), nvs[i]);
            }
        }
        public static Uri Uri
        {
            get
            {
                var appId = WebResource.Instance().Provider["appId"];
                if (String.IsNullOrEmpty(appId) == false)
                {
                    String code = Utility.Parse36Encode(Utility.Guid(appId).Value);
                    // return new Uri($"http://127.0.0.1:5188/{code}/");
                    return new Uri($"https://api.apiumc.com/{code}/");
                }
                else
                {
                    return new Uri("https://api.apiumc.com/0/");
                }
            }
        }

        public static String Api = "https://ali.apiumc.cn/";

        static APIProxy()
        {
            var appId = UMC.Data.WebResource.Instance().Provider["api"];
            if (String.IsNullOrEmpty(appId) == false)
            {
                try
                {
                    Api = new Uri(new Uri(appId), "/").AbsoluteUri;
                }
                catch
                {

                }
            }
        }

        public static Uri Wxwork(string pathQuery)
        {
            return new Uri($"{Api}wxwork/{pathQuery}");

        }

        public static Uri WeiXin(string pathQuery)
        {
            return new Uri($"{Api}WeiXin/{pathQuery}");

        }

        // public static Uri Url(string key, string pathQuery)
        // {
        //     return new Uri($"{Api}{key}/{pathQuery}");

        // }
        public static Uri DingTalk(string pathQuery)
        {
            return new Uri($"{Api}dd/{pathQuery}");

        }
    }
}

