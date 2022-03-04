using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace UMC.Web
{


    /// <summary>
    /// 请求的单据
    /// </summary>
    public class WebClient
    {
        class CommandKey
        {
            public string cmd
            {
                get;
                set;
            }
            public string model
            {
                get;
                set;
            }
            public string value
            {
                get;
                set;
            }

        }

        internal const int OuterDataEvent = 131072;

        /// <summary>
        /// 扩展
        /// </summary>
        internal static readonly WebEvent Prompt = (WebEvent)2048;

        public bool? IsVerify
        {
            get;
            set;
        }
        internal WebRuntime _runtime;
        public void Clear(WebEvent Event)
        {
            if ((this.ClientEvent & Event) == Event)
            {
                this.ClientEvent = this.ClientEvent ^ Event;
            }
            switch (Event)
            {
                case WebEvent.Normal:
                    break;
            }
        }

        internal System.IO.Stream InputStream
        {
            get
            {
                return this._context.InputStream;
            }
        }

        System.Collections.IDictionary InnerHeaders
        {
            get;
            set;
        }
        public System.Collections.Hashtable OuterHeaders
        {
            get;
            private set;
        }

        public WebEvent ClientEvent
        {
            get;
            set;
        }
        int RedirectTimes = 0;
        public WebSession Session
        {
            get;
            private set;
        }

        public Uri Uri
        {
            get;
            set;
        }
        public string UserHostAddress
        {
            get;
            set;
        }
        public Uri UrlReferrer
        {
            get;
            set;
        }

        public bool IsApp { get; set; }
        public UMC.Security.AccessToken Token { get; set; }
        public bool IsCashier { get; set; }
        public string UserAgent
        {
            get;
            set;
        }

        static bool CheckApp(String UserAgent, String query)
        {

            if (String.IsNullOrEmpty(UserAgent) == false)
            {
                if (UserAgent.Contains("UMC Client") == false)
                {
                    if (String.IsNullOrEmpty(query) == false)
                    {
                        return query.Contains("&_v=Debug");
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        Net.NetContext _context;
        public WebClient(Net.NetContext context, WebSession manager = null)
        {
            this._context = context;
            this.Token = context.Token;
            this.Uri = context.Url;
            this.UserHostAddress = context.UserHostAddress;
            this.UrlReferrer = context.UrlReferrer;
            this.UserAgent = context.UserAgent;// System.Uri.UnescapeDataString(userAgent);
            this.Session = manager ?? WebSession.Instance();
            this.InnerHeaders = UMC.Data.JSON.Deserialize<Hashtable>(this.Session.Header(context.Token)) ?? new Hashtable();
            if (this.InnerHeaders.Contains("Click"))
            {
                this.UIEvent = Data.JSON.Deserialize<UIClick>(this.InnerHeaders["Click"] as string);

            }
            this.IsCashier = context.Token.IsInRole(UMC.Security.Membership.UserRole);
            this.IsApp = CheckApp(this.UserAgent, this.Uri.Query);//.Contains("&_v=Debug");

        }

        internal int XHRTime = 0;
        public void JSONP(string json, string p, System.IO.TextWriter writer)
        {
            var cmds = UMC.Data.JSON.Deserialize<CommandKey[]>(json);

            if (String.IsNullOrEmpty(p) == false)
            {
                writer.Write(p);
                writer.Write('(');
            }
            if (cmds != null)
            {
                writer.Write('{');
                var h = true;
                foreach (var c in cmds)
                {
                    if (this.OuterHeaders != null)
                    {
                        this.OuterHeaders.Clear();
                    }
                    this.ClientEvent = WebEvent.None;
                    if (String.IsNullOrEmpty(c.value))
                    {
                        this.Command(c.model, c.cmd, String.Empty);
                    }
                    else if (c.value.IndexOf("=") > -1)
                    {
                        var QueryString = System.Web.HttpUtility.ParseQueryString(c.value) ?? new NameValueCollection();
                        this.Command(c.model, c.cmd, QueryString);
                    }
                    else
                    {
                        this.Command(c.model, c.cmd, c.value);
                    }

                    if (h)
                    {
                        h = false;
                    }
                    else
                    {
                        writer.Write(",");

                    }
                    if (String.IsNullOrEmpty(c.value))
                    {
                        UMC.Data.JSON.Serialize(String.Format("{0}.{1}", c.model, c.cmd), writer);
                    }
                    else
                    {
                        UMC.Data.JSON.Serialize(String.Format("{0}.{1}.{2}", c.model, c.cmd, c.value), writer);
                    }
                    writer.Write(":");
                    this.WriteTo(writer, ur => { });

                }
                if (h == false)
                {
                    writer.Write(",");
                }
                writer.Write("\"TimeSpan\":");
                writer.Write(Data.Utility.TimeSpan());
                writer.Write("}");
            }
            if (String.IsNullOrEmpty(p) == false)
            {
                writer.Write(")");
            }
        }
        public void Start(string start)
        {
            this.ClientEvent = WebEvent.Normal;
            this.Send(null);
        }


        void Redirect(string model, string cmd, WebMeta meta)
        {
            if (Verify(model, cmd) == false)
            {
                return;
            }
            this.InnerHeaders.Clear();
            if (meta != null)
            {
                this.InnerHeaders["Arguments"] = meta.GetDictionary();
            }
            this.ModelCommand(model, cmd, this.InnerHeaders);
            this.Send();
        }
        public void Command(string model, string cmd, string value)
        {

            if (Verify(model, cmd) == false)
            {
                return;
            }

            ClearUIEvent(model, cmd);
            Redirect(model, cmd, value);



        }
        void Redirect(string model, string cmd, string value)
        {
            this.InnerHeaders.Clear();
            var hash = this.InnerHeaders;
            if (!String.IsNullOrEmpty(value))
            {
                //hash[model] = value;
                hash[cmd] = value;
            }
            this.ModelCommand(model, cmd, hash);
            this.Send();

        }
        void ModelCommand(string model, string cmd, System.Collections.IDictionary header)
        {
            header["POS-MODEL"] = model;
            header["POS-COMMAND"] = cmd;
        }


        bool Verify(string model, string cmd)
        {
            if (this.IsVerify.HasValue == false)
            {
                this.IsVerify = this.Session.IsAuthorization(model, cmd);
                if (this.IsVerify == true)
                {
                    return true;
                }
                String key = String.Format("{0}.{1}", model, cmd);
                WebAuthType authType = WebAuthType.Check;
                if (WebRuntime.authKeys.ContainsKey(key))
                {
                    authType = WebRuntime.authKeys[key];
                }
                else if (WebRuntime.authKeys.ContainsKey(model))
                {
                    authType = WebRuntime.authKeys[model];
                }
                var user = this.Token.Identity();// UMC.Security.Identity.Current;
                System.Security.Principal.IPrincipal principal = user;// WebADNuke.Security.Identity.Current;



                switch (authType)
                {
                    case WebAuthType.All:
                        this.IsVerify = true;
                        return true;
                    case WebAuthType.User:
                        if (principal.IsInRole(Security.Membership.UserRole))
                        {
                            this.IsVerify = true;
                            return true;

                        }
                        break;
                    case WebAuthType.UserCheck:
                        if (principal.IsInRole(Security.Membership.AdminRole))
                        {
                            this.IsVerify = true;
                            return true;

                        }
                        else if (principal.IsInRole(Security.Membership.UserRole))
                        {

                            if (UMC.Security.AuthManager.IsAuthorization(principal, String.Format("{0}/{1}", model, cmd)))
                            {
                                this.IsVerify = true;
                                return true;
                            }
                        }
                        break;
                    case WebAuthType.Check:
                        if (principal.IsInRole(Security.Membership.AdminRole))
                        {
                            this.IsVerify = true;
                            return true;

                        }
                        else if (user.IsAuthenticated)
                        {
                            if (UMC.Security.AuthManager.IsAuthorization(principal, String.Format("{0}/{1}", model, cmd)))
                            {
                                this.IsVerify = true;
                                return true;
                            }

                        }

                        break;
                    case WebAuthType.Admin:
                        if (principal.IsInRole(Security.Membership.AdminRole))
                        {
                            this.IsVerify = true;
                            return true;

                        }
                        break;
                    case WebAuthType.Guest:
                        if (user.IsAuthenticated)
                        {
                            this.IsVerify = true;
                            return true;

                        }
                        else
                        {
                            this.OuterHeaders = new Hashtable();
                            this.ClientEvent = WebEvent.Prompt | WebEvent.DataEvent;
                            this.OuterHeaders["Prompt"] = new WebMeta().Put("Title", "提示", "Text", "您没有登录,请登录");

                            this.OuterHeaders["DataEvent"] = new WebMeta().Put("type", "Login");
                            return false;
                        }
                }

                this.OuterHeaders = new Hashtable();
                this.ClientEvent = WebEvent.Prompt; //| WebEvent.DataEvent;
                if (principal.IsInRole(Security.Membership.UserRole) == false)
                {
                    this.OuterHeaders["Prompt"] = new WebMeta().Put("Title", "提示", "Text", "您没有登录或权限受限");
                    this.ClientEvent = WebEvent.Prompt | WebEvent.DataEvent;
                    this.OuterHeaders["DataEvent"] = new WebMeta().Put("type", "Close");
                }
                else
                {
                    this.OuterHeaders["Prompt"] = new WebMeta().Put("Title", "提示", "Text", "您的权限受限,请与管理员联系");

                }
                return false;

            }
            return true;
        }

        internal UIClick UIEvent
        {
            get; set;
        }

        /// <summary>
        /// 当前处理共享健值
        /// </summary>
        internal System.Collections.Hashtable Items = new System.Collections.Hashtable();
        bool isSave = false;
        void ClearUIEvent(string model, string cmd)
        {
            if (this.UIEvent != null)
            {

                var pModel = this.InnerHeaders["POS-MODEL"] as string;
                var pCommand = this.InnerHeaders["POS-COMMAND"] as string;
                if (String.Equals(pModel, model) == false || String.Equals(pCommand, cmd) == false)
                {
                    this.InnerHeaders.Clear();

                    this.UIEvent = null;
                    this.isSave = true;


                }
            }

        }
        public void Command(string model, string cmd, NameValueCollection QueryString)
        {

            if (Verify(model, cmd) == false)
            {
                return;
            }
            ClearUIEvent(model, cmd);

            Redirect(model, cmd, QueryString);



        }
        void Redirect(string model, string cmd, NameValueCollection QueryString)
        {

            switch (QueryString.Count)
            {
                case 0:
                    this.Redirect(model, cmd, String.Empty);
                    break;
                case 1:
                    var skey = QueryString.GetKey(0);
                    var svalue = QueryString.Get(0);
                    if (String.IsNullOrEmpty(skey))
                    {
                        this.Redirect(model, cmd, svalue);

                    }
                    else if (String.IsNullOrEmpty(svalue))
                    {

                        this.Redirect(model, cmd, skey);
                    }
                    else
                    {

                        goto default;
                    }
                    break;
                default:
                    var hash = new System.Collections.Hashtable();
                    for (var i = 0; i < QueryString.Count; i++)
                    {
                        var key = QueryString.GetKey(i);
                        var value = QueryString.Get(i);
                        if (String.IsNullOrEmpty(key) == false)
                        {
                            hash[key] = value;
                        }
                        else
                        {
                            hash["Id"] = value;
                        }
                    }
                    this.InnerHeaders.Clear();
                    var header = this.InnerHeaders;
                    header[model] = hash;

                    this.ModelCommand(model, cmd, header);
                    this.Send();
                    break;
            }

        }

        public void SendDialog(string value)
        {
            var header = this.InnerHeaders;


            header["Dialog"] = value;

            this.Send();


        }

        void Send()
        {
            if (this.IsVerify.HasValue == false)
            {
                this.IsVerify = true;
            }
            Send(this.InnerHeaders);
        }
        void OutputHeader(UMC.Web.WebMeta header)
        {
            OutputHeader(header, -1);
        }
        public void OutputHeader(UMC.Web.WebMeta header, int index)
        {
            if (this.OuterHeaders == null)
            {
                this.OuterHeaders = new Hashtable(); ;
            }

            var dic = header.GetDictionary();

            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                var key = em.Key.ToString();
                if (key == "DataEvent")
                {
                    var value = em.Value;
                    if (this.OuterHeaders.ContainsKey(key))
                    {
                        var ats = new System.Collections.ArrayList();
                        var ts = this.OuterHeaders[key];

                        if (ts is Array)
                        {
                            if (index == -1)
                            {

                                ats.AddRange((Array)ts);
                            }
                            else
                            {

                                ats.InsertRange(index, (Array)ts);
                            }

                        }
                        else
                        {
                            if (index == -1)
                            {

                                ats.Add(ts);
                            }
                            else
                            {
                                ats.Insert(index, ts);

                            }
                        }
                        if (value is Array)
                        {
                            if (index == -1)
                            {
                                ats.AddRange((Array)value);
                            }
                            else
                            {
                                ats.InsertRange(index, (Array)value);

                            }

                        }
                        else
                        {
                            if (index == -1)
                            {

                                ats.Add(value);
                            }
                            else
                            {
                                ats.Insert(index, value);

                            }

                        }
                        this.OuterHeaders[key] = ats.ToArray();
                    }
                    else
                    {
                        this.OuterHeaders[em.Key] = em.Value;
                    }
                }
                else
                {
                    this.OuterHeaders[em.Key] = em.Value;
                }
            }

        }
        public void SendDialog(NameValueCollection QueryString)
        {
            switch (QueryString.Count)
            {
                case 0:
                    this.Start("true");
                    break;
                case 1:
                    var sKey = QueryString.GetKey(0);
                    var sValue = QueryString.Get(0);
                    if (String.IsNullOrEmpty(sKey) == false && String.IsNullOrEmpty(sValue) == false)
                    {
                        goto default;
                    }
                    else if (String.IsNullOrEmpty(sValue) == false)
                    {
                        this.SendDialog(sValue);
                    }
                    else if (String.IsNullOrEmpty(sKey) == false)
                    {

                        this.SendDialog(sKey);
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                default:
                    var header = this.InnerHeaders;
                    var sn = header["POS-SN"];

                    var Dialog = new System.Collections.Hashtable();
                    for (var i = 0; i < QueryString.Count; i++)
                    {
                        if (String.IsNullOrEmpty(QueryString.GetKey(i)))
                        {
                            continue;
                        }
                        Dialog[QueryString.GetKey(i)] = QueryString[i];
                    }
                    header["Dialog"] = Dialog;
                    Send();
                    break;
            }


        }
        internal bool IsEmptyReq = false;

        ClientRedirect _lastRedirect;
        void Send(System.Collections.IDictionary doc2)
        {
            this.IsEmptyReq = false;
            var context = doc2 == null ? WebRuntime.Start(this) : WebRuntime.ProcessRequest(doc2, this);
            var response = context.Response;
            if (IsEmptyReq)
            {
                _lastRedirect = null;
            }

            this.Session.Check(context);
            this.RedirectTimes++;
            var clientEvent = response.ClientEvent;
            this.InnerHeaders.Clear();
            this.ModelCommand(context.Request.Model, context.Request.Command, this.InnerHeaders);


            if ((Convert.ToInt32(clientEvent) & OuterDataEvent) == OuterDataEvent)
            {

                this.ClientEvent = clientEvent;
                this.OutputHeader(response.Headers);
                return;
            }

            var clientRedirect = response.ClientRedirect;
            if ((clientEvent & WebEvent.AsyncDialog) == WebEvent.AsyncDialog)
            {
                this.InnerHeaders[WebRequest.KEY_HEADER_ARGUMENTS] = response.Headers.GetMeta(WebRequest.KEY_HEADER_ARGUMENTS);
                if (clientRedirect != null)
                {
                    if ((clientEvent & WebClient.Prompt) != WebClient.Prompt)
                    {
                        this.ModelCommand(clientRedirect.Model, clientRedirect.Command, this.InnerHeaders);
                        clientRedirect = response.ClientRedirect = null;
                    }
                }
                if ((clientEvent & WebEvent.Dialog) == WebEvent.Dialog)
                {
                    clientEvent ^= WebEvent.Dialog;

                    this.InnerHeaders["POS-DIALOG"] = "YES";
                }
            }
            if (clientRedirect != null)
            {
                if (this.RedirectTimes > 10)
                {
                    throw new Exception(String.Format("{0}.{1},请求重定向超过最大次数", clientRedirect.Model, clientRedirect.Command));
                }
                var args = response.Headers.GetMeta(WebRequest.KEY_HEADER_ARGUMENTS);
                _lastRedirect = clientRedirect;

                if (clientEvent != WebEvent.None)
                {

                    this.ClientEvent |= clientEvent;
                    response.Headers.Remove(WebRequest.KEY_HEADER_ARGUMENTS);
                    OutputHeader(response.Headers);
                }

                if (String.IsNullOrEmpty(response.ClientRedirect.Value))
                {
                    this.Redirect(clientRedirect.Model, clientRedirect.Command, args);
                }
                else
                {
                    if (clientRedirect.Value.IndexOf("&") > -1)
                    {
                        var nquery = System.Web.HttpUtility.ParseQueryString(clientRedirect.Value);
                        this.Redirect(clientRedirect.Model, clientRedirect.Command, nquery);
                    }
                    else if (clientRedirect.Value.StartsWith("{"))
                    {
                        var p = UMC.Data.JSON.Deserialize(clientRedirect.Value) as Hashtable;
                        var pos = new NameValueCollection();
                        var em = p.GetEnumerator();
                        while (em.MoveNext())
                        {
                            pos[em.Key.ToString()] = em.Value.ToString();
                        }
                        this.Redirect(clientRedirect.Model, clientRedirect.Command, pos);
                    }
                    else
                    {
                        this.Redirect(clientRedirect.Model, clientRedirect.Command, clientRedirect.Value);
                    }
                }
                return;
            }
            response.Headers.Remove(WebRequest.KEY_HEADER_ARGUMENTS);
            OutputHeader(response.Headers);
            this.ClientEvent = this.ClientEvent | clientEvent;
            if (IsEmptyReq == false)
            {
                if (_lastRedirect != null && XHRTime > 1)
                {
                    this.ClientEvent = this.ClientEvent | WebEvent.Reset;
                    response.Headers.Put("Reset", _lastRedirect);
                }
            }
        }
        //



        public WebContext Context
        {
            get
            {
                return _runtime.Context;
            }
        }
        public void WriteTo(System.IO.TextWriter writer, Action<Uri> redirect)
        {
            if ((Convert.ToInt32(this.ClientEvent) & OuterDataEvent) == OuterDataEvent)
            {
                var data = this.OuterHeaders["Data"];

                if (data is WebClient)
                {
                    var d = data as WebClient;

                    d.WriteTo(writer, redirect);
                }
                else if (data is System.Uri)
                {
                    redirect(data as System.Uri);
                }
                else if (data is WebFactory.XHRer)
                {
                    this.Session.Storage(this.InnerHeaders, this.Context);
                    UMC.Data.JSON.Serialize(data, writer);
                    return;
                }
                else
                {
                    UMC.Data.JSON.Serialize(data, writer);
                }
                if (this.isSave)
                {
                    this.Session.Storage(this.InnerHeaders, this.Context);
                }
                return;
            }
            if (this.UIEvent != null)
            {
                this.InnerHeaders["Click"] = Data.JSON.Serialize(this.UIEvent);
            }
            if ((this.ClientEvent & WebEvent.AsyncDialog) == WebEvent.AsyncDialog)
            {
                this.Session.Storage(this.InnerHeaders, _runtime.Context);

            }
            else if (this.isSave || (this.ClientEvent & WebEvent.Header) == WebEvent.Header)
            {
                this.Session.Storage(this.InnerHeaders, _runtime.Context);
            }

            IDictionary<String, object> outer = null;
            if (this._runtime != null)
            {
                outer = this.Session.Outer(this, _runtime.Context);
            }
            writer.Write('{');
            writer.Write("\"ClientEvent\":{0}", Convert.ToInt32(this.ClientEvent));
            if (this.OuterHeaders != null && this.OuterHeaders.Count > 0)
            {

                writer.Write(",\"Headers\":");
                UMC.Data.JSON.Serialize(this.OuterHeaders, writer);
            }
            if (outer != null)
            {
                var em = outer.GetEnumerator();
                while (em.MoveNext())
                {
                    writer.Write(",");
                    UMC.Data.JSON.Serialize(em.Current.Key, writer);
                    writer.Write(":");
                    UMC.Data.JSON.Serialize(em.Current.Value, writer);

                }
            }
            writer.Write('}');
        }

    }

}
