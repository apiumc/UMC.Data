using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using System.IO;
using System.Threading.Tasks;
using System.Collections;

namespace UMC.Web
{
    class WebRuntime : IDisposable
    {
        public Hashtable Items
        {
            get
            {
                return Client.Items;
            }
        }
        public WebRuntime(WebClient client, System.Collections.IDictionary header)
        {
            this.Request = client.Session.Request();
            this.Request.OnInit(client, header);
            this.Response = client.Session.Response();
            this.Response.OnInit(client);
            this.Client = client;
        }
        ~WebRuntime()
        {
            GC.SuppressFinalize(this);
        }
        public WebClient Client
        {
            get;
            set;
        }
        static bool isScanning()
        {
            return WebRuntime.webFactorys.Count > 0 || WebRuntime.activities.Count > 0 || WebRuntime.flows.Count > 0;
        }
        static WebRuntime()
        {
            var sb = new StringBuilder();
            sb.AppendLine("                                                                     ");
            sb.AppendLine("                                                                     ");
            sb.AppendLine("    $$         $$           $$$$$$    $$$$$$             $$$$$$$$    ");
            sb.AppendLine("    $$         $$         $$      $$$$      $$         $$            ");
            sb.AppendLine("    $$         $$        $$        $$        $$       $$             ");
            sb.AppendLine("    $$         $$        $$        $$        $$       $$             ");
            sb.AppendLine("    $$         $$        $$        $$        $$       $$             ");
            sb.AppendLine("    $$         $$        $$        $$        $$       $$             ");
            sb.AppendLine("    $$         $$        $$        $$        $$       $$             ");
            sb.AppendLine("     $$       $$         $$        $$        $$        $$            ");
            sb.AppendLine("       $$$$$$$           $$        $$        $$          $$$$$$$$    ");
            sb.AppendLine("                                                                     ");
            sb.AppendLine("                                                                     ");
            //Console.Write(sb);
            var dics = new System.IO.DirectoryInfo(System.Environment.CurrentDirectory).GetFiles("*.dll", SearchOption.TopDirectoryOnly);

            var last = DateTime.Now;// Utility.TimeSpan();// dic.LastWriteTime);
            foreach (var f in dics)
            {

                if (last > f.LastWriteTime)
                {
                    last = f.LastWriteTime;
                }
            }

            String mapFile = Utility.MapPath("App_Data/register.net");
            var lastTime = Utility.TimeSpan(last);
            String m = Utility.Reader(mapFile);
            if (String.IsNullOrEmpty(m) == false)
            {
                Hashtable map = JSON.Deserialize(m) as Hashtable;
                if (map.ContainsKey("time"))
                {
                    if (Utility.IntParse(map["time"].ToString(), 0) == lastTime)
                    {
                        Array mapings = map["data"] as Array;
                        if (mapings != null)
                        {
                            int l = mapings.Length;
                            for (int i = 0; i < l; i++)
                            {
                                String v = mapings.GetValue(i) as string;
                                WebRuntime.Register(Type.GetType(v));
                            }
                        }
                    }
                }
            }
            if (isScanning() == false)
            {
                Reflection.Instance().ScanningClass();
                Utility.Writer(mapFile, JSON.Serialize(new WebMeta().Put("time", lastTime).Put("data", WebRuntime.RegisterCls())), false);
            }
            WebRuntime.webFactorys.Sort((x, y) => y.Weight.CompareTo(x.Weight));
            var em = WebRuntime.flows.GetEnumerator();
            while (em.MoveNext())
            {
                em.Current.Value.Sort((x, y) => y.Weight.CompareTo(x.Weight));

            }
        }
        public static WebContext Start(WebClient client)
        {
            WebRuntime runtime = new WebRuntime(client, new Hashtable());
            Current = runtime;
            runtime.Context = client.Session.Context();
            try
            {
                runtime.Context.Init(runtime);
                runtime.Context.OnInit(client);
            }
            catch (UMC.Web.WebAbortException)
            {
            }

            return runtime.Context;

        }
        public static WebContext ProcessRequest(System.Collections.IDictionary header, WebClient client)
        {
            WebRuntime runtime = new WebRuntime(client, header);

            var value = System.Threading.Thread.CurrentPrincipal;
            runtime.DoFactory(value);
            return runtime.Context;

        }
        [ThreadStatic]
        static WebRuntime _Current;
        /// <summary>
        /// 当前的处理工厂
        /// </summary>
        public static WebRuntime Current
        {
            get
            {
                return _Current;
            }
            private set
            {
                _Current = value;
            }
        }

        public static List<String> RegisterCls()
        {
            List<String> metas = new List<string>();

            if (WebRuntime.webFactorys.Count > 0)
            {
                foreach (WeightType wt in WebRuntime.webFactorys)
                {
                    var t = wt.Type;
                    metas.Add(t.FullName + "," + t.Assembly.FullName);
                }
            }
            if (WebRuntime.flows.Count > 0)
            {
                var em = WebRuntime.flows.GetEnumerator();
                while (em.MoveNext())
                {
                    var fs = em.Current.Value;
                    foreach (var wt in fs)
                    {

                        var t = wt.Type;
                        metas.Add(t.FullName + "," + t.Assembly.FullName);
                    }
                }
            }
            if (WebRuntime.activities.Count > 0)
            {
                var em = WebRuntime.activities.GetEnumerator();
                while (em.MoveNext())
                {
                    //var fs = em.Current.Value;
                    var mv = em.Current.Value.GetEnumerator();
                    while (mv.MoveNext())
                    {
                        var t = mv.Current.Value;

                        metas.Add(t.FullName + "," + t.Assembly.FullName);
                    }
                }
            }
            foreach (Type t in UMC.Data.Sql.Initializer.initializers)
            {
                metas.Add(t.FullName + "," + t.Assembly.FullName);
            }


            return metas;

        }


        internal WebFlow CurrentFlow
        {
            get;
            set;
        }
        internal IWebFactory CurrentFlowFactory
        {
            get;
            set;
        }

        /// <summary>
        /// POS交易上下文
        /// </summary>
        public WebContext Context
        {
            get;
            set;
        }

        public WebActivity CurrentActivity
        {
            get;
            set;
        }
        //static WebRuntime()
        //{

        //    // System.IO.DirectoryInfo
        //}
        public static void Register(Type t)
        {
            if (t == null)
            {
                return;
            }
            var tUid = t.GUID;
            var mpps = t.GetCustomAttributes(typeof(MappingAttribute), false);
            foreach (var m in mpps)
            {
                var mp = m as MappingAttribute;
                if (String.IsNullOrEmpty(mp.Command) == false && String.IsNullOrEmpty(mp.Model) == false)
                {
                    if (typeof(WebActivity).IsAssignableFrom(t))
                    {
                        if (activities.ContainsKey(mp.Model) == false)
                        {
                            activities.Add(mp.Model, new Dictionary<string, Type>());
                        }
                        var cmd = String.Format("{0}.{1}", mp.Model, mp.Command);
                        var actDic = activities[mp.Model];


                        if (weightKeys.ContainsKey(cmd))
                        {

                            if (weightKeys[cmd] >= mp.Weight)
                            {
                                continue;
                            }
                        }
                        if (mp.Category > 0)
                        {
                            Categorys.Add(mp);
                        }
                        if (mp.Weight > 0)
                        {
                            weightKeys[cmd] = mp.Weight;
                        }

                        activities[mp.Model][mp.Command] = t;
                        authKeys[cmd] = mp.Auth;
                        //if (mp.IsLog)
                        //{
                        //    IsLog[cmd] = mp.Desc;
                        //}
                    }
                }
                else if (String.IsNullOrEmpty(mp.Model) == false)
                {
                    if (typeof(WebFlow).IsAssignableFrom(t))
                    {
                        if (flows.ContainsKey(mp.Model) == false)
                        {
                            flows.Add(mp.Model, new List<WeightType>()); ;
                        }
                        var list = flows[mp.Model];
                        if (list.Exists(e => e.Type.GUID == tUid) == false)
                        {
                            list.Add(new WeightType { Type = t, Weight = mp.Weight });
                            authKeys[mp.Model] = mp.Auth;

                        }
                    }

                }
                else if (typeof(IWebFactory).IsAssignableFrom(t))
                {
                    if (webFactorys.Exists(e => e.Type.GUID == tUid) == false)
                    {

                        if (typeof(WebFactory).IsAssignableFrom(t))
                        {
                            webFactorys.Insert(0, new WeightType { Type = t, Weight = 10000 });
                        }
                        else
                        {
                            webFactorys.Add(new WeightType { Type = t, Weight = mp.Weight });
                        }
                    }
                }
                else if (typeof(UMC.Data.Sql.Initializer).IsAssignableFrom(t))
                {
                    if (UMC.Data.Sql.Initializer.initializers.Exists(e => e.GUID == tUid) == false)
                        UMC.Data.Sql.Initializer.initializers.Add(t);
                }


            }
        }

        public static void Register(System.Reflection.Assembly assembly)
        {

            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                Register(t);
            }
        }
        public class WeightType
        {
            public Type Type { get; set; }
            public int Weight
            {
                get; set;
            }
        }

        internal static List<MappingAttribute> Categorys = new List<MappingAttribute>();
        internal static Dictionary<String, WebAuthType> authKeys = new Dictionary<String, WebAuthType>();
        internal static Dictionary<String, int> weightKeys = new Dictionary<String, int>();
        internal static List<WeightType> webFactorys = new List<WeightType>();
        internal static Dictionary<String, List<WeightType>> flows = new Dictionary<string, List<WeightType>>();
        internal static Dictionary<String, Dictionary<String, Type>> activities = new Dictionary<String, Dictionary<string, Type>>();

        class MappingFLow : WebFlow
        {

            public override WebActivity GetFirstActivity()
            {
                var webRequest = this.Context.Request;
                var dic = activities[webRequest.Model];
                if (dic.ContainsKey(webRequest.Command))
                {
                    return Reflection.CreateInstance(dic[webRequest.Command]) as WebActivity;

                }
                else
                {
                    return WebActivity.Empty;
                }
            }
        }
        class MappingActivityFactory : IWebFactory
        {
           // int index = 0;

            WebFlow IWebFactory.GetFlowHandler(string mode)
            {
                if (activities.ContainsKey(mode))
                {

                    return new MappingFLow();

                }
                return WebFlow.Empty;

            }

            void IWebFactory.OnInit(WebContext context)
            {

            }
        }
        class MappingFlowFactory : IWebFactory
        {
            int index = 0;
            public MappingFlowFactory(int i)
            {
                this.index = i;
            }
            WebFlow IWebFactory.GetFlowHandler(string mode)
            {
                if (flows.ContainsKey(mode))
                {
                    return Reflection.CreateInstance(flows[mode][index].Type) as WebFlow;
                }
                return WebFlow.Empty;

            }

            void IWebFactory.OnInit(WebContext context)
            {

            }
            public static IWebFactory[] GetFactory(String model)
            {
                if (flows.ContainsKey(model))
                {
                    var len = flows[model].Count;
                    var list = new List<IWebFactory>();
                    //list.Sort((x,y)=>x.)
                    for (var i = 0; i < len; i++)
                    {
                        list.Add(new MappingFlowFactory(i));

                    }
                    return list.ToArray();
                }
                return new IWebFactory[0];

            }
        }
        void DoFactory()
        {

            Context = this.Client.Session.Context();

            Context.Init(this);
            Context.OnInit(this.Client);
            var factorys = new List<IWebFactory>();


            factorys.Add(new MappingActivityFactory());

            factorys.AddRange(MappingFlowFactory.GetFactory(Context.Request.Model));
            int webIndex = 0;
            foreach (var ftype in webFactorys)
            {
                var flowFactory = Reflection.CreateInstance(ftype.Type) as IWebFactory;
                if (flowFactory != null)
                {

                    flowFactory.OnInit(Context);

                    if (flowFactory is WebFactory)
                    {
                        factorys.Insert(0, flowFactory);
                        webIndex++;
                    }
                    else
                    {
                        factorys.Add(flowFactory);
                    }
                }
            }
            var webf = new WebFactory();
            webf.OnInit(Context);

            factorys.Insert(webIndex, webf);

            foreach (var factory in factorys)
            {
                this.CurrentFlowFactory = factory;

                var flow = factory.GetFlowHandler(this.Request.Model);
                flow.Context = this.Context;

                this.CurrentFlow = flow;

                ProcessActivity(flow, flow.GetFirstActivity());
            }
            Context.Complete();
        }
        protected void DoFactory(System.Security.Principal.IPrincipal pi)
        {
            System.Threading.Thread.CurrentPrincipal = pi;
            WebRuntime.Current = this;

            if ((this.Response.ClientEvent & WebEvent.Error) == WebEvent.Error)
            {
                return;
            }
            if (String.Equals(Reflection.Instance().Provider["debug"], "true") == false)
            {
                try
                {
                    DoFactory();

                }
                catch (UMC.Web.WebAbortException)
                {
                }
                catch (Exception ex)
                {
                    this.Response.ClientEvent = WebEvent.Error;
                    this.Response.Headers["Error"] = ex.Message;
                    UMC.Data.Utility.Error("UMC", DateTime.Now, this.Request.Url.AbsoluteUri, ex.ToString());
                }

            }
            else
            {
                try
                {
                    DoFactory();

                }
                catch (UMC.Web.WebAbortException)
                {
                }
            }

        }

        void ProcessActivity(WebFlow flow, WebActivity active)
        {
            active.Flow = flow;
            active.Context = this.Context;
            this.CurrentActivity = active;
            if (active == WebActivity.Empty)
            {
            }
            else
            {
                this.Client.IsEmptyReq = true;
                active.ProcessActivity(this.Request, this.Response);
                ProcessActivity(flow, flow.GetNextActivity(active.Id));
            }
        }
        public WebRequest Request
        {
            get;
            private set;
        }
        public WebResponse Response
        {
            get;
            private set;
        }


        #region IDisposable Members

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
