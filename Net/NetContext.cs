using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using UMC.Data;
using System.Threading;

namespace UMC.Net
{
    public class Comparer : EqualityComparer<String>
    {
        public override bool Equals(String x, String y)
        {
            return String.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode(String obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }
    public abstract class NetContext
    {
        public abstract int StatusCode
        {
            get;
            set;
        }
        public abstract string ContentType
        {
            get;
            set;
        }
        public abstract long? ContentLength
        {
            get;
            set;
        }
        public abstract string UserHostAddress
        {
            get;
        }
        public abstract string RawUrl
        {
            get;
        }
        public abstract string UserAgent
        {
            get;
        }
        public abstract void AddHeader(string name, string value);
        public abstract void AppendCookie(string name, string value);
        public abstract void AppendCookie(string name, string value, string path);
        public abstract NameValueCollection Headers
        {
            get;
        }

        public abstract NameValueCollection QueryString
        {
            get;
        }
        public abstract NameValueCollection Cookies
        {
            get;
        }
        public virtual UMC.Security.AccessToken Token
        {
            get;
            protected set;
        }
        public abstract NameValueCollection Form
        {
            get;
        }

        public abstract void ReadAsData(Net.NetReadData readData);

        public abstract System.IO.Stream InputStream
        {
            get;
        }
        public abstract bool AllowSynchronousIO
        {
            get;
        }
        public virtual void UseSynchronousIO(Action action)
        {

        }
        public virtual void OutputFinish()
        {

        }
        public virtual void Error(Exception ex)
        {
            //throw ex;
        }
        public abstract System.IO.TextWriter Output
        {
            get;
        }

        public abstract System.IO.Stream OutputStream
        {
            get;
        }
        public abstract Uri UrlReferrer
        {
            get;
        }
        public abstract Uri Url
        {
            get;
        }
        public abstract string HttpMethod
        {
            get;
        }
        public abstract void Redirect(string url);

        public static void Authorization(UMC.Net.NetContext context, bool isNewCookie)
        {
            var sessionKey = context.Cookies[Security.Membership.SessionCookieName];

            string contentType = "Desktop";
            var isApp = false;
            if (UMC.Data.Utility.IsApp(context.UserAgent))
            {
                contentType = "App";
            }

            var ns = new NameValueCollection();
            var sign = String.Empty;
            var hs = context.Headers;
            for (var i = 0; i < hs.Count; i++)
            {
                var key = hs.GetKey(i).ToLower();
                switch (key)
                {
                    case "umc-request-sign":
                        sign = hs[i];
                        break;
                    default:
                        if (key.StartsWith("umc-"))
                        {
                            ns.Add(key, Uri.UnescapeDataString(hs[i]));
                        }
                        break;
                }
            }
            if (String.IsNullOrEmpty(sign) == false)
            {
                if (String.Equals(Data.Utility.Sign(ns, Data.WebResource.Instance().AppSecret()), sign, StringComparison.CurrentCultureIgnoreCase))
                {
                    var roles = ns["umc-request-user-role"];
                    var id = ns["umc-request-user-id"];
                    var name = ns["umc-request-user-name"];
                    var alias = ns["umc-request-user-alias"];

                    var sid = new Guid(UMC.Data.Utility.MD5("umc.request", id, name, roles, alias));

                    var session = new Session<Security.AccessToken>(sid.ToString());
                    if (session.Value != null)
                    {
                        var auth = session.Value;
                        var passDate = Data.Utility.TimeSpan();

                        if (((auth.ActiveTime ?? 0) + auth.Timeout) < passDate)
                        {
                            var iden = auth.Identity();
                            if (String.Equals(iden.Name, name))
                            {
                                context.Token = auth;
                                return;
                            }
                        }
                    }
                    var user = UMC.Security.Membership.Instance().Identity(name);

                    if (user == null)
                    {
                        user = UMC.Security.Identity.Create(Utility.Guid(id) ?? Utility.Guid(name, true).Value, name, alias, String.IsNullOrEmpty(roles) ? new String[0] : roles.Split(','));
                    }
                    var token = UMC.Security.AccessToken.Create(user, sid, 3600);
                    token.Commit(contentType, context.UserHostAddress);
                    context.Token = token;

                    return;
                }
            }

            if (String.IsNullOrEmpty(sessionKey))
            {
                var uid = Guid.NewGuid();
                sessionKey = Utility.Guid(uid);
                context.AppendCookie(Security.Membership.SessionCookieName, sessionKey);

                context.Token = UMC.Security.AccessToken.Create(new UMC.Security.Guest(uid), uid, 0);
                return;


            }
            else if (isNewCookie)
            {
                var deviceId = Data.Utility.Guid(sessionKey, true).Value;

                context.Token = Security.AccessToken.Create(new UMC.Security.Guest(deviceId), deviceId, 0);
                return;
            }
            else
            {
                var deviceId = Data.Utility.Guid(sessionKey, true).Value;

                var session = new Session<Security.AccessToken>(deviceId.ToString());
                if (session.Value != null && session.Value.Id == deviceId)
                {

                    var auth = session.Value;
                    auth.Id = deviceId;
                    //auth.ContentType = contentType;
                    if (auth.UId != deviceId)
                    {
                        if (isApp == false && context.UserHostAddress.IndexOf('/') > 0 && String.Equals(session.ClientIP, context.UserHostAddress) == false)
                        {
                            context.Token = Security.AccessToken.Create(new UMC.Security.Guest(deviceId), deviceId, 0); ;
                            return;
                        }
                    }
                    var passDate = Data.Utility.TimeSpan();
                    if (auth.Timeout > 0)
                    {
                        if (((auth.ActiveTime ?? 0) + auth.Timeout) < passDate)
                        {
                            var at = UMC.Security.AccessToken.Create(new UMC.Security.Guest(deviceId), deviceId, 0);
                            at.Commit(contentType, context.UserHostAddress);
                            context.Token = at;

                            return;

                        }
                    }
                    if (auth.ActiveTime < passDate - 600)
                    {
                        auth.Commit(contentType, context.UserHostAddress);
                    }
                    context.Token = auth;
                    return;
                }

                context.Token = Security.AccessToken.Create(new UMC.Security.Guest(deviceId), deviceId, 0); ;


            }

        }
    }


}
