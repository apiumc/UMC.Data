using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 请求的单据
    /// </summary>
    public class WebRequest
    {
        public bool IsCashier
        {
            get
            {
                return client.IsCashier;
            }

        }
        bool? _IsMaster;
        public bool IsMaster
        {
            get
            {
                if (_IsMaster.HasValue == false)
                {
                    _IsMaster = UMC.Security.Principal.Current.IsInRole(UMC.Security.Membership.AdminRole);
                }
                return _IsMaster.Value;
            }

        }
        public string UserAgent
        {
            get
            {
                return client.UserAgent;
            }
        }
        bool? _IsWeiXin;
        public bool IsWeiXin
        {
            get
            {
                if (_IsWeiXin.HasValue == false)
                {
                    if (String.IsNullOrEmpty(this.UserAgent) == false)
                    {
                        this._IsWeiXin = this.UserAgent.IndexOf("MicroMessenger") > 10;
                    }
                }
                return _IsWeiXin ?? false;
            }
        }

        internal const string KEY_HEADER_ARGUMENTS = "Arguments", KEY_ARGUMENTS_ITEMS = "KEY_ARGUMENTS_ITEMS";

        /// <summary>
        /// 是否是消费者App
        /// </summary>
        public bool IsApp
        {
            get
            {
                return client.IsApp;
            }
        }
        public System.IO.Stream InputStream
        {
            get
            {
                return client.InputStream;
            }
        }
        private WebClient client;


        internal protected virtual void OnInit(WebClient client, System.Collections.IDictionary header)
        {
            
            this.Model = header["POS-MODEL"] as string;
            this.Command = header["POS-COMMAND"] as string;
            header.Remove("POS-MODEL");
            header.Remove("POS-COMMAND");

            var he = new WebMeta();
            he.Set(header);
            this._Headers = he;
            this.Arguments = this._Headers.GetMeta(KEY_HEADER_ARGUMENTS) ?? new WebMeta();
            if (this.Arguments.ContainsKey(KEY_ARGUMENTS_ITEMS) == false)
            {
                this.Items = new WebMeta();

            }
            else
            {
                this.Items = this.Arguments.GetMeta(KEY_ARGUMENTS_ITEMS) ?? new WebMeta();
            }
            this.Arguments.Remove(KEY_ARGUMENTS_ITEMS);

            this.client = client;
        }


        /// <summary>
        /// 模式下的指令
        /// </summary>
        public string Command
        {
            get;
            private set;
        }
        /// <summary>
        /// 提交的值
        /// </summary>
        public string SendValue
        {
            get
            {
                return this._Headers.Get(this.Command);
            }
        }
        /// <summary>
        /// 提交的值
        /// </summary>
        public WebMeta SendValues
        {
            get
            {
                return this._Headers.GetMeta(this.Model);
            }
        }


        /// <summary>
        /// 请求Sesion
        /// </summary>
        public WebMeta Items
        {
            get;
            private set;
        }
        /// <summary>
        /// 参数
        /// </summary>
        public WebMeta Arguments
        {
            get;
            private set;
        }
        WebMeta _Headers;
        /// <summary>
        /// 头信息
        /// </summary>
        public WebMeta Headers
        {
            get
            {
                return _Headers;
            }
        }
        /// <summary>
        /// 模块
        /// </summary>
        public string Model
        {
            get;
            private set;
        }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                return client.UserHostAddress;
            }
        }
        /// <summary>
        /// 请求的引用
        /// </summary>
        public Uri UrlReferrer
        {
            get
            {
                return client.UrlReferrer;
            }
        }

        public Uri Url
        {
            get
            {
                return client.Uri;
            }
        }
    }
}
