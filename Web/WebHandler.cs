using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UMC.Web
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WebHandler
    {

        /// <summary>
        /// 上下文的会话
        /// </summary>
        public WebContext Context
        {
            get;
            internal set;
        }
        /// <summary>
        /// 当前处理流程
        /// </summary>
        public WebFlow Flow
        {
            get;
            internal set;
        }

        /// <summary>
        /// 提示
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="endResponse">是否结束响应返回客户端</param>

        protected void Prompt(string text, bool endResponse)
        {
            WebResponse response = this.Context.Response;
            response.ClientEvent |= WebEvent.Prompt;
            WebMeta prompt = new WebMeta();
            prompt["Text"] = text;

            response.Headers.Set("Prompt", prompt);
            if (endResponse)
            {
                this.Context.End();
            }

        }
        /// <summary>
        /// 选项请求
        /// </summary>
        /// <param name="asyncId">对话框Id</param>
        /// <param name="model">选项模块</param>
        /// <param name="cmd">选项指令</param>
        public string AsyncDialog(String asyncId, String model, String cmd)
        {
            return this.AsyncDialog(asyncId, g =>
             {
                 var click = new UIClick(Context.Request.Arguments.Put(asyncId, "Value")).Send(Context.Request.Model, Context.Request.Command); ;
                 this.Context.runtime.Client.UIEvent = click;
                 var data = new Hashtable();
                 data["Click"] = Data.JSON.Serialize(click);

                 data["POS-MODEL"] = model;
                 data["POS-COMMAND"] = cmd;
                 this.Context.runtime.Client.Session.Storage(data, Context);
                 Context.Response.Redirect(model, cmd, "Click", true);
                 return UMC.Web.UIDialog.ReturnValue(asyncId);
             });

        }
        public string AsyncDialog(String asyncId, String model, String cmd, WebMeta meta)
        {
            return this.AsyncDialog(asyncId, g =>
            {
                var click = new UIClick(Context.Request.Arguments.Put(asyncId, "Value")).Send(Context.Request.Model, Context.Request.Command); ;
                this.Context.runtime.Client.UIEvent = click;
                var data = new Hashtable();
                data["Click"] = Data.JSON.Serialize(click);

                data["POS-MODEL"] = model;
                data["POS-COMMAND"] = cmd;
                this.Context.runtime.Client.Session.Storage(data, Context);
                Context.Response.Redirect(model, cmd, meta.Put("Key", "Click"), true);
                return UMC.Web.UIDialog.ReturnValue(asyncId);
            });

        }
        /// <summary>
        /// 单值对话框
        /// </summary>
        protected string AsyncDialog(string asyncId, AsyncDialogCallback callback, bool isDialog)
        {
            return AsyncDialog(asyncId, callback, isDialog);
        }
        protected string AsyncDialog(string asyncId, string deValue)
        {
            return UIDialog.AsyncDialog(asyncId, k => this.DialogValue(deValue));
        }

        protected UIDialog DialogValue(string value)//, WebADNuke.Web.AsyncDialogCallback callback, bool isDialog)
        {
            return UIDialog.ReturnValue(value);
        }

        protected UMC.Web.UIFormDialog DialogValue(WebMeta value)//, WebADNuke.Web.AsyncDialogCallback callback, bool isDialog)
        {
            return UMC.Web.UIDialog.ReturnValue(value);
        }
        /// <summary>
        /// 单值对话框
        /// </summary>
        /// <param name="asyncId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected string AsyncDialog(string asyncId, UMC.Web.AsyncDialogCallback callback)
        {
            return UMC.Web.UIDialog.AsyncDialog(asyncId, callback);
        }
        /// <summary>
        /// 表单对话框
        /// </summary>
        protected WebMeta AsyncDialog(UMC.Web.AsyncDialogFormCallback callback, string asyncId)
        {
            return UIFormDialog.AsyncDialog(asyncId, d => callback(asyncId));
        }
        /// <summary>
        /// 表单对话框
        /// </summary>
        protected WebMeta AsyncDialog(string asyncId, UMC.Web.AsyncDialogFormCallback callback)
        {
            return UIFormDialog.AsyncDialog(asyncId, d => callback(asyncId));
        }
        /// <summary>
        /// 提示框,并终止响应且返回客户端
        /// </summary>
        /// <param name="text">文本</param>
        protected void Prompt(string text)
        {

            Prompt(text, true);
        }
        /// <summary>
        /// 弹出式提示框,并终止响应且返回客户端
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="text">文本</param>
        protected void Prompt(string title, string text)
        {
            this.Prompt(title, text, true);
        }
        /// <summary>
        /// 弹出式提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="text">文本</param>
        /// <param name="endResponse">是否结束响应返回客户端</param>
        protected void Prompt(string title, string text, bool endResponse)
        {
            var meta = new WebMeta().Put("Type", "Prompt").Put("Title", title).Put("Text", text);

            var response = this.Context.Response;
            response.Headers.Set(EventType.AsyncDialog, meta);
            response.ClientEvent |= WebEvent.AsyncDialog | WebClient.Prompt;
            if (endResponse)
            {
                this.Context.End();
            }
        }
    }
}
