using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using UMC.Data;

namespace UMC.Web
{
    /// <summary>
    /// 交易会话存储方案
    /// </summary>
    public abstract class WebSession
    {
        private class WebSessioner : WebSession
        {
            public WebSessioner()
            {

                this.Header = Security.AccessToken.Get("WebSession");
            }

            protected internal override void Check(WebContext context)
            {

            }

            protected internal override bool IsAuthorization(string model, string command)
            {

                return false;
            }

            protected internal override IDictionary<string, object> Outer(WebClient client, WebContext context)
            {
                return null;
            }

            protected internal override void Storage(IDictionary header, WebContext context)
            {
                Security.AccessToken.Set("WebSession", JSON.Serialize(header));


            }
        }
        public static void Instance(Type webSession)
        {
            _instance = webSession;
        }
        static Type _instance = typeof(WebSessioner);
        public static WebSession Instance()
        {
            if (_instance != null)
            {
                return UMC.Data.Reflection.CreateInstance(_instance) as WebSession;
            };
            return UMC.Data.Reflection.CreateObject("WebSession") as WebSession ?? new WebSessioner();
        }

        /// <summary>
        /// 指令验证
        /// </summary>
        /// <param name="model"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        internal protected abstract bool IsAuthorization(string model, string command);



        internal protected abstract void Check(WebContext context);
        /// <summary>
        /// 存储会员的数据
        /// </summary>
        /// <param name="header">请求头</param>
        /// <param name="ticket">单据</param>
        /// <param name="sessionData">Session的数据</param>
        internal protected abstract void Storage(System.Collections.IDictionary header, WebContext context);
        internal protected abstract IDictionary<String, Object> Outer(WebClient client, WebContext context);



        public virtual string Header
        {
            get;
            protected set;
        }
        public virtual WebRequest Request()
        {
            return new WebRequest();
        }
        public virtual WebResponse Response()
        {
            return new WebResponse();
        }
        public virtual WebContext Context()
        {
            return new WebContext();
        }
    }
}
