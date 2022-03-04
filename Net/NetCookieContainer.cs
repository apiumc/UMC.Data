using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using UMC.Web;

namespace UMC.Net
{
    public class NetCookieContainer : System.Net.CookieContainer, UMC.Data.IJSON
    {
        public NetCookieContainer()
        {
            _cookies = new List<WebMeta>();
        }
        Action<Cookie> action;
        public NetCookieContainer(Action<Cookie> action)
        {
            _cookies = new List<WebMeta>();
            this.action = action;

        }
        public new int Count
        {
            get
            {
                return _cookies.Count;
            }
        }
        public NetCookieContainer(List<WebMeta> cookies)
        {
            this._cookies = cookies;
        }
        List<WebMeta> _cookies = new List<WebMeta>();
        public new string GetCookieHeader(Uri uri)
        {
            var sb = new StringBuilder();
            var ts = UMC.Data.Utility.TimeSpan();
            var absolutePath = uri.AbsolutePath;
            foreach (var ck in _cookies)
            {
                var path = ck["path"] ?? "/";
                if (absolutePath.StartsWith(path))
                {
                    if (ck.ContainsKey("expires"))
                    {
                        var expires = UMC.Data.Utility.IntParse(ck["expires"], 0) - 60 * 5;
                        if (expires > ts)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append("; ");
                            }
                            sb.AppendFormat("{0}={1}", ck["name"], ck["value"]);
                        }
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("; ");
                        }
                        sb.AppendFormat("{0}={1}", ck["name"], ck["value"]);
                    }
                }
            }
            return sb.ToString();
        }

        public new void Add(Cookie cookie)
        {
            var ckey = String.Format("{0}{1}", cookie.Name, cookie.Path);

            var cookie2 = new WebMeta();
            cookie2.Put("name", cookie.Name, "value", cookie.Value, "path", cookie.Path ?? "/");
            _cookies.RemoveAll(d => String.Equals(String.Format("{0}{1}", d["name"], d["path"]), ckey));
            if (cookie.Expires > DateTime.Now)
            {
                cookie2.Put("expires", UMC.Data.Utility.TimeSpan(cookie.Expires).ToString());
            }
            _cookies.Add(cookie2);
        }

        public Cookie GetCookie(String name)
        {
          
            foreach (var ck in _cookies)
            {
                var key = ck["name"];
                if (String.Equals(key, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    var path = ck["path"] ?? "/";

                    if (ck.ContainsKey("expires"))
                    {
                        var expires = UMC.Data.Utility.IntParse(ck["expires"], 0);
                        return new Cookie(ck["name"], ck["value"], path)
                        {
                            Expires = UMC.Data.Utility.TimeSpan(expires)
                        };
                    }
                    else
                    {
                        return new Cookie(ck["name"], ck["value"], path);

                    }
                }
            }
            return null;
        }
        #region IJSONSerializable Members

        void Data.IJSON.Write(System.IO.TextWriter writer)
        {
            writer.Write('[');
            var isHave = false;
            var ts = UMC.Data.Utility.TimeSpan();
            foreach (var ck in _cookies)
            {

                if (ck.ContainsKey("expires"))
                {
                    var expires = UMC.Data.Utility.IntParse(ck["expires"], 0) - 60 * 5;
                    if (expires > ts)
                    {
                        if (isHave)
                        {
                            writer.Write(',');
                        }
                        isHave = true;
                        UMC.Data.JSON.Serialize(ck, writer);
                    }
                }
                else
                {
                    if (isHave)
                    {
                        writer.Write(',');
                    }
                    isHave = true;
                    UMC.Data.JSON.Serialize(ck, writer);
                }
            }

            writer.Write(']');
        }

        void Data.IJSON.Read(string key, object value)
        {

        }


        #endregion
        public new void SetCookies(Uri uri, string cookieHeader)
        {
            var cheades = cookieHeader.Split(';');

            var chv = cheades[0];
            var vIndex = chv.IndexOf('=');
            var name = chv.Substring(0, vIndex);

            var cookie = new Cookie(chv.Substring(0, vIndex), chv.Substring(vIndex + 1));
            for (var i = 1; i < cheades.Length; i++)
            {
                var sv = cheades[i].Split('=');
                switch (sv[0].ToLower().Trim())
                {
                    case "path":
                        cookie.Path = sv[1];
                        break;
                    case "expires":
                        cookie.Expires = Convert.ToDateTime(sv[1]);
                        break;
                    case "max-age":
                        cookie.Expires = DateTime.Now.AddSeconds(UMC.Data.Utility.IntParse(sv[1], 0));
                        break;
                }
            }
            if (String.IsNullOrEmpty(cookie.Path) == false)
            {
                var ckey = String.Format("{0}{1}", cookie.Name, cookie.Path);
                _cookies.RemoveAll(d => String.Equals(String.Format("{0}{1}", d["name"], d["path"]), ckey));
                if (cookie.Expires == DateTime.MinValue)
                {
                    _cookies.Add(new WebMeta().Put("name", name, "value", cookie.Value, "path", cookie.Path));

                }
                else
                {
                    _cookies.Add(new WebMeta().Put("name", name, "value", cookie.Value, "path", cookie.Path).Put("expires", UMC.Data.Utility.TimeSpan(cookie.Expires).ToString()));
                }
            }

            if (this.action != null)
            {
                this.action(cookie);
            }
        }
    }
}

