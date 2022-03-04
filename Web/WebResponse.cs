using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Reflection;
using UMC.Net;

namespace UMC.Web
{
    /// <summary>
    /// 交易
    /// </summary>
    public class WebResponse
    {
        public WebResponse()
        {
            this.Headers = new WebMeta();
        }
        WebClient _client;
        internal protected virtual void OnInit(WebClient client)
        {
            this._client = client;
        }
        //internal WebContext Context
        //{
        //    get;
        //    private set;
        //}

        /// <summary>
        /// 参数信息
        /// </summary>
        internal WebMeta Headers
        {
            get;
            private set;
        }
        /// <summary>
        /// 响应客户端事件
        /// </summary>
        public WebEvent ClientEvent
        {
            get;
            set;
        }
        internal ClientRedirect ClientRedirect
        {
            get;
            set;
        }
        /// <summary>
        /// 重新请求
        /// </summary>
        /// <param name="mode">模块</param>
        /// <param name="cmd">命令</param>
        public void Redirect(string mode, string cmd)
        {
            this.Redirect(mode, cmd, true);
        }
        /// <summary>
        /// 重新请求
        /// </summary>
        /// <param name="mode">模块</param>
        /// <param name="cmd">命令</param>
        /// <param name="value">值</param>
        /// <param name="endResponse">是否结束响应返回客户端</param>
        public void Redirect(string mode, string cmd, string value, bool endResponse)
        {
            this.ClientRedirect = new ClientRedirect { Model = mode, Command = cmd, Value = value };
            if (endResponse)
            {
                this.End();
            }
        }
        /// <summary>
        /// 重新请求
        /// </summary>
        /// <param name="mode">模块</param>
        /// <param name="cmd">命令</param>
        /// <param name="value">值</param>
        public void Redirect(string mode, string cmd, string value)
        {
            this.Redirect(mode, cmd, value, true);
        }

        /// <summary>
        /// 重新请求
        /// </summary>
        /// <param name="mode">模块</param>
        /// <param name="cmd">命令</param>
        /// <param name="endResponse">是否结束响应返回客户端</param>
        public void Redirect(string mode, string cmd, bool endResponse)
        {
            this.ClientRedirect = new ClientRedirect { Model = mode, Command = cmd };
            if (endResponse)
            {
                this.End();
            }
        }
        /// <summary>
        /// 输出数据
        /// </summary>
        /// <param name="data">输出的数据</param>
        public void Redirect(object data)
        {
            this.Headers.GetDictionary()["Data"] = data;
            this.ClientEvent |= (WebEvent)WebClient.OuterDataEvent;
            this.End();

        }
        /// <summary>
        /// 重新请求
        /// </summary>
        /// <param name="mode">模块</param>
        /// <param name="cmd">命令</param>
        /// <param name="dialog">对话框</param>
        public void Redirect(string mode, string cmd, UMC.Web.UIDialog dialog)
        {
            this.Headers.Set(EventType.AsyncDialog, dialog.ToAsyncArgs(_client.Context));
            this.ClientEvent |= WebEvent.AsyncDialog;
            this.ClientRedirect = new ClientRedirect { Model = mode, Command = cmd };
            this.End();

        }
        /// <summary>
        /// 重新请求
        /// </summary>
        /// <param name="mode">模块</param>
        /// <param name="cmd">命令</param>
        /// <param name="arguments">参数</param>
        public void Redirect(string mode, string cmd, WebMeta arguments, bool endResponse)
        {
            this.ClientRedirect = new ClientRedirect { Model = mode, Command = cmd, Value = Data.JSON.Serialize(arguments) };

            if (endResponse)
            {
                this.End();
            }
        }
        internal void RedirectDialog(string mode, string cmd, UMC.Web.UIDialog dialog, WebRequest req)
        {
            this.Headers.Set(EventType.AsyncDialog, dialog.ToAsyncArgs(_client.Context));

            if (req.Items.Count > 0)
            {
                req.Arguments.Set(WebRequest.KEY_ARGUMENTS_ITEMS, req.Items);
            }

            this.Headers.Set(WebRequest.KEY_HEADER_ARGUMENTS, req.Arguments);
            this.ClientEvent |= WebEvent.AsyncDialog | WebEvent.Dialog;
            this.ClientRedirect = new ClientRedirect { Model = mode, Command = cmd };

            this.End();

        }
        public void End()
        {

            throw new WebAbortException();
        }


    }
}
