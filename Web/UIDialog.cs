using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{

    /// <summary>
    /// 异步单值对话框，回调产生的对话框
    /// </summary>
    /// <returns></returns>
    public delegate UIDialog AsyncDialogCallback(string asyncId);

    /// <summary>
    /// 异步多值调对话框，回调产生的对话框
    /// </summary>
    /// <returns></returns>
    public delegate UIFormDialog AsyncDialogFormCallback(string asyncId);

    /// <summary>
    /// 对话框
    /// </summary>
    public abstract class UIDialog
    {

        class POSFromValue : UIFormDialog
        {
            protected override string DialogType
            {
                get { throw new NotImplementedException(); }
            }
            /// </summary>
            public WebMeta InputValues
            {

                get;
                set;
            }
        }
        class POSDialogValue : UIDialog
        {
            public string InputValue
            {
                get;
                set;
            }
            protected override string DialogType
            {
                get { throw new NotImplementedException(); }
            }
        }
        class APOSDialog : UIDialog
        {
            //public object Values
            //{
            //    get;
            //    set;
            //}
            public APOSDialog(string type)
            {
                this._DType = type;
            }
            String _DType;
            protected override string DialogType
            {
                get { return _DType; }
            }
        }
        class aGridDialog : UIGridDialog
        {
            Header header;
            public aGridDialog(Header header, object data)
            {
                this.IsAsyncData = true;
                this.header = header;
                this.data = data;
            }
            protected override Hashtable GetHeader()
            {
                return header.GetHeader();
            }
            object data;
            protected override Hashtable GetData(IDictionary paramsKey)
            {
                var hash = new Hashtable();
                hash["data"] = data;
                return hash;
            }
        }
        public static UIGridDialog Create(UIGridDialog.Header header, System.Data.DataTable data, bool isReturn)
        {
            return new aGridDialog(header, data) { IsReturnValue = isReturn };
        }
        public static UIGridDialog Create(UIGridDialog.Header header, Array data, bool isReturn)
        {
            return new aGridDialog(header, data) { IsReturnValue = isReturn };
        }
        public static UIGridDialog Create(UIGridDialog.Header header, System.Data.DataTable data)
        {
            return new aGridDialog(header, data);
        }
        public static UIGridDialog Create(UIGridDialog.Header header, Array data)
        {
            return new aGridDialog(header, data);
        }
        public static UIDialog CreateDialog(string type)
        {
            return new APOSDialog(type);
        }
        public static UIDialog CreateImage(string title, Uri uri, string tip)
        {
            var p = new APOSDialog("Image");
            p.Title = title;
            p.Config["Url"] = uri.AbsoluteUri;
            if (String.IsNullOrEmpty(tip) == false)
            {
                p.Config["Text"] = tip;
            }
            return p;
        }
        //public WebMeta CreateMenu(string text, string model, string cmd, string value)
        //{
        //    var p = new WebMeta();
        //    p["model"] = model;
        //    if (String.IsNullOrEmpty(value) == false)
        //    {
        //        p["send"] = value;
        //    }
        //    p["text"] = text;
        //    p["cmd"] = cmd;
        //    return p;
        //}
        //public static WebMeta CreateMenu(string text, string model, string cmd, WebMeta param)
        //{

        //    var p = new WebMeta();
        //    if (param != null)
        //    {
        //        p.Set("send", param);
        //    }
        //    p["model"] = model;
        //    p["text"] = text;
        //    p["cmd"] = cmd;
        //    return p;
        //}
        protected bool IsAsyncData
        {
            get;
            set;
        }

        /// <summary>
        /// 对话框Id
        /// </summary>
        protected string AsyncId
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建不用返回值客户端直接返回对话框
        /// </summary>
        /// <param name="value">对话框要返回的值，只能是String，或POSMeta类型</param>
        /// <returns></returns>
        public static UIDialog ReturnValue(string value)
        {
            var v = new POSDialogValue();

            v.InputValue = value as string;
            return v;

        }
        /// <summary>
        /// 创建不用返回值客户端直接返回对话框
        /// </summary>
        /// <param name="value">对话框要返回的值，只能是String，或POSMeta类型</param>
        /// <returns></returns>
        public static UIFormDialog ReturnValue(WebMeta value)
        {
            var v = new POSFromValue();

            v.InputValues = value as WebMeta;

            return v;

        }
        const string Dialog = "Dialog";
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected abstract string DialogType
        {
            get;
        }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue
        {
            get;
            set;
        }
        public String RefreshEvent
        {
            get
            {
                return config["RefreshEvent"];
            }
            set
            {
                config["RefreshEvent"] = value;
            }
        }
        public String CloseEvent
        {
            get
            {
                return config["CloseEvent"];
            }
            set
            {
                config["CloseEvent"] = value;
            }
        }
        WebMeta config = new WebMeta();
        /// <summary>
        /// 对话框参数配置
        /// </summary>
        public WebMeta Config
        {
            get
            {
                return config;
            }
        }
        /// <summary>
        /// 对话框标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// 获取异步对话框的值
        /// </summary>
        /// <param name="asyncId">异步值Id</param>
        /// <param name="dialog">对话框</param>
        public static string AsyncDialog(WebContext context, string asyncId, UIDialog dialog)
        {
            return GetAsyncValue(context, asyncId, true, aid => dialog, false) as string;
        }
        /// <summary>
        /// 获取异步对话框的值
        /// </summary>
        /// <param name="asyncId">异步值Id</param>
        /// <param name="callback">对话框回调方法</param>
        public static string AsyncDialog(WebContext context, string asyncId, AsyncDialogCallback callback)
        {
            return GetAsyncValue(context, asyncId, true, callback, false) as string;
        }
        //public static string (string asyncId, AsyncDialogCallback callback)
        //{
        //    return GetAsyncValue(asyncId, true, callback, false) as string;
        //}
        public static string AsyncDialog(WebContext context, string asyncId, AsyncDialogCallback callback, bool IsDialogValue)
        {
            return GetAsyncValue(context, asyncId, true, callback, IsDialogValue) as string;
        }
        internal const string KEY_DIALOG_ID = "KEY_DIALOG_ID";
        protected static object GetAsyncValue(WebContext context, string asyncId, bool singleValue, AsyncDialogCallback callback, bool IsDialog)
        {
            //var context = WebContext.Current;
            var request = context.Request;
            var response = context.Response;
            if (singleValue)
            {
                var rValue = request.Arguments.Get(asyncId);
                if (String.IsNullOrEmpty(rValue))
                {
                    var value = request.Headers[EventType.Dialog];
                    var isSVs = false;

                    if (String.IsNullOrEmpty(value) && request.Headers.ContainsKey(EventType.Dialog))
                    {
                        var meValue = request.Headers.GetMeta(EventType.Dialog);
                        if (meValue != null)
                        {
                            value = meValue[asyncId];
                        }
                    }
                    if (String.IsNullOrEmpty(value))
                    {
                        if (IsDialog == false)
                        {
                            if (context.runtime.Items.ContainsKey(context.CurrentActivity) == false)
                            {
                                value = request.SendValue;
                            }
                            if (String.IsNullOrEmpty(value))
                            {
                                var obj = request.Headers.GetDictionary()[request.Model];

                                if (obj is WebMeta)
                                {
                                    var mob = ((WebMeta)obj);
                                    value = mob[asyncId];
                                    mob.Remove(asyncId);
                                    isSVs = true;
                                }
                                else if (obj is IDictionary)
                                {
                                    var idc = ((IDictionary)obj);
                                    value = idc[asyncId] as string;
                                    idc.Remove(asyncId);
                                    isSVs = true;
                                }
                            }
                        }

                    }

                    if (String.IsNullOrEmpty(value) == false)
                    {
                        if (isSVs)
                        {
                            request.Arguments[(asyncId)] = value;
                            return value;
                        }
                        else if (context.runtime.Items.ContainsKey(context.CurrentActivity) == false)
                        {
                            context.runtime.Items.Add(context.CurrentActivity, true);
                            request.Arguments[(asyncId)] = value;
                            return value;
                        }
                    }
                    var dialog = callback(asyncId);
                    dialog.AsyncId = asyncId;
                    if (dialog is POSDialogValue)
                    {
                        var dv = dialog as POSDialogValue;
                        request.Arguments[(asyncId)] = dv.InputValue;
                        return dv.InputValue;
                    }

                    response.RedirectDialog(request.Model, request.Command, dialog, request);
                }
                return rValue;
            }
            else
            {
                var rValue = request.Arguments.GetMeta(asyncId);
                if (rValue == null)
                {
                    rValue = request.Headers.GetMeta(EventType.Dialog);//?? request.SendValues;

                    if (rValue == null)
                    {
                        if (request.SendValues != null && request.SendValues.ContainsKey(KEY_DIALOG_ID))
                        {
                            if (request.SendValues[KEY_DIALOG_ID] == asyncId)
                            {
                                rValue = new WebMeta(request.SendValues.GetDictionary());
                                rValue.Remove(KEY_DIALOG_ID);
                                var em = request.Arguments.GetDictionary().GetEnumerator();
                                while (em.MoveNext())
                                {
                                    rValue.Remove(em.Key as string);
                                }
                            }
                        }
                    }

                    if (rValue == null || context.runtime.Items.ContainsKey(context.CurrentActivity))
                    {
                        var dialog = callback(asyncId);
                        dialog.AsyncId = asyncId;
                        if (dialog is POSFromValue)
                        {
                            var dfv = dialog as POSFromValue;
                            request.Arguments.Set(asyncId, dfv.InputValues);
                            return dfv.InputValues;
                        }
                        response.RedirectDialog(request.Model, request.Command, dialog, request);
                    }
                    else
                    {
                        context.runtime.Items.Add(context.CurrentActivity, true);
                        request.Arguments.Set(asyncId, rValue); ;
                    }
                }
                return rValue;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Initialization(WebContext context) { }

        /// <summary>
        /// 转化异步参数
        /// </summary>
        /// <returns></returns>
        internal WebMeta ToAsyncArgs(WebContext context)
        {
            Initialization(context);
            if (!String.IsNullOrEmpty(this.DefaultValue))
            {
                this.config["DefaultValue"] = DefaultValue;
            }
            if (!String.IsNullOrEmpty(Title))
            {
                this.config["Title"] = Title;
            }
            this.config["Type"] = DialogType;
            return this.config;
        }

        /// <summary>
        /// 用异步对话框
        /// </summary>
        public static string AsyncDialog(WebContext context, string valueKey, string title)
        {
            return AsyncDialog(context, valueKey, anyc => new UITextDialog { Title = title });
        }


    }
}
