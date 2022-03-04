using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 表单对话框
    /// </summary> 
    public class UIFormDialog : UIDialog
    {
        List<WebMeta> dataSrouce = new List<WebMeta>();// new POSMeta();
        /// <summary>
        /// 类型
        /// </summary>
        protected override string DialogType
        {
            get { return "Form"; }
        }
        ///// <summary>
        ///// 增加地址输入框
        ///// </summary>
        ///// <param name="title"></param>
        ///// <param name="Code"></param>
        ///// <param name="defaultValue"></param>
        //public WebMeta AddReceiver(string Code, string title, string defaultValue)
        //{
        //    WebMeta v = new WebMeta();
        //    v["Title"] = title;
        //    v["DefaultValue"] = defaultValue;
        //    v["Type"] = "Receiver";
        //    v["Name"] = Code;
        //    this.dataSrouce.Add(v);
        //    return v;
        //}
        public WebMeta CreateMenu(string text, string model, string cmd, string value)
        {
            var p = new WebMeta();
            p["model"] = model;
            if (String.IsNullOrEmpty(value) == false)
            {
                p["send"] = value;
            }
            p["text"] = text;
            p["cmd"] = cmd;
            return p;
        }
        public void Menu(string text, string model, string cmd, string value)
        {
            this.Menu(this.CreateMenu(text, model, cmd, value));
        }
        public void Menu(string text, string model, string cmd)
        {
            this.Menu(this.CreateMenu(text, model, cmd, ""));
        }
        public void Menu(params WebMeta[] menus)
        {
            this.Config.Put("menu", menus);
        }
        public WebMeta CreateMenu(string text, string model, string cmd, WebMeta param)
        {

            var p = new WebMeta();
            if (param != null)
            {
                p.Set("send", param);
            }
            p["model"] = model;
            p["text"] = text;
            p["cmd"] = cmd;
            return p;
        }
        public void Menu(string text, string model, string cmd, WebMeta param)
        {
            this.Menu(this.CreateMenu(text, model, cmd, param));
        }
        public void Add(UICell cell)
        {
            this.dataSrouce.Add(new WebMeta().Put("Type", cell.Type).Put("value", cell.Data).Put("format", cell.Format).Put("style", cell.Style));

        }
        /// <summary>
        /// 获取异步对话框的值
        /// </summary>
        /// <param name="asyncId">异步值Id</param>
        /// <param name="callback">对话框回调方法</param>
        /// <returns></returns>
        public static new WebMeta AsyncDialog(WebContext context, string asyncId, AsyncDialogCallback callback)
        {
            return GetAsyncValue(context, asyncId, false, callback, false) as WebMeta;
        }
        /// <summary>
        /// 增加地址输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public void AddArea(string title, string Code, string defaultValue)
        {
            this.AddOption(title, Code, defaultValue, defaultValue).Put("Model", "Settings", "Command", "Area");
        }
        public void AddSlider(string title, string Code, int defaultValue, int min, int max)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue.ToString();
            v["Type"] = "FieldSlider";
            v["Name"] = Code;
            v["Max"] = max.ToString();
            v["Min"] = min.ToString(); ;
            this.dataSrouce.Add(v);
        }
        public void AddSlider(string title, string Code, int defaultValue)
        {
            AddSlider(title, Code, defaultValue, 0, 100);
        }
        /// <summary>
        /// 增加地址输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public void AddAddress(string title, string Code, string defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;
            v["Type"] = "Address";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加电话输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public void AddPhone(string title, string Code, string defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;
            v["Type"] = "Number";
            v["Vtype"] = "Phone";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加数字输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddNumber(string title, string Code, int? defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue.ToString();
            v["Type"] = "Number";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
        }

        public WebMeta Add(String type, String Code, String title, String defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;//.ToString();
            v["Type"] = type;// "Number";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;

        }
        /// <summary>
        /// 增加数字输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public void AddNumber(string title, string Code, decimal? defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue.ToString();
            v["Type"] = "Number";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加数字输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public void AddNumber(string title, string Code, float? defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue.ToString();
            v["Type"] = "Number";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加数字输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddNumber(string title, string Code, string defaultValue = "")
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;
            v["Type"] = "Number";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 增加确认提示
        /// </summary>
        /// <param name="caption"></param>
        public void AddConfirm(string caption)
        {
            this.AddConfirm(caption, null, null);
        }
        /// <summary>
        /// 增加确认提示
        /// </summary>
        /// <param name="caption"></param>
        public void AddConfirm(string caption, string name, string defaultValue)
        {
            WebMeta v = new WebMeta();
            //v["Title"] = title;
            v["Text"] = caption;
            v["Type"] = "Confirm";
            v["DefaultValue"] = defaultValue ?? "YES";
            v["Name"] = name ?? "CONFIRM_NAME";// WebADNuke.Utils.Utility.Parse62Encode(Guid.NewGuid().GetHashCode());
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加提示，
        /// </summary>
        /// <param name="caption"></param>
        public void AddPrompt(string caption)
        {
            WebMeta v = new WebMeta();
            //v["Title"] = title;
            v["Text"] = caption;
            v["Type"] = "Prompt";
            v["Name"] = UMC.Data.Utility.Parse62Encode(Guid.NewGuid().GetHashCode());
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 条码输入框，
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddBarCode(string title, string Code, string defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;
            v["Type"] = "BarCode";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
            //this.dataSrouce.Set(Code, v);
        }
        /// <summary>
        /// 列表选择框，
        /// </summary>
        /// <param name="title"></param>
        /// <param name="code"></param>
        /// <param name="value"></param>
        public WebMeta AddOption(string title, string code, string value, String text)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v.Put("Text", text).Put("DefaultValue", value);
            v["Type"] = "Option";
            v["Name"] = code;
            this.dataSrouce.Add(v);
            return v;
        }
        public WebMeta AddOption(string title, string code)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["Type"] = "Option";
            v["Name"] = code;
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 文件上传，
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddFile(string title, string Code, string defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["Type"] = "File";
            v["Name"] = Code;
            if (String.IsNullOrEmpty(defaultValue) == false)
            {
                v["DefaultValue"] = defaultValue;
            }
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 文件上传，
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddFiles(string title, string Code)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["Type"] = "Files";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 多行文本输入，
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddTextarea(string title, string Code, string defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["Type"] = "Textarea";
            if (String.IsNullOrEmpty(defaultValue) == false)
            {
                v["DefaultValue"] = defaultValue;
            }
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 时间输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddDate(string title, string code, DateTime? defaultValue)
        {

            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["Name"] = code;
            if (defaultValue.HasValue)
            {
                v["DefaultValue"] = defaultValue.Value.ToString("yyyy-MM-dd");
            }
            v["Type"] = "Date";
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 增加文本输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddText(string title, string Code, string defaultValue = "")
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;
            v["Type"] = "Text";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
            //this.dataSrouce.Set(Code, v);
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public WebMeta AddTextValue(string title, ListItemCollection items)
        {
            WebMeta v = new WebMeta();
            if (String.IsNullOrEmpty(title) == false)
                v["Title"] = title;
            v.GetDictionary()["DataSource"] = items;

            v["Type"] = "TextValue";
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public WebMeta AddTextValue(ListItemCollection items)
        {
            return this.AddTextValue(String.Empty, items);
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public WebMeta AddTextNameValue(ListItemCollection items)
        {
            return this.AddTextNameValue(String.Empty, items);
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public WebMeta AddTextNameValue(string title, ListItemCollection items)
        {
            WebMeta v = new WebMeta();
            if (String.IsNullOrEmpty(title) == false)
                v["Title"] = title;
            v.GetDictionary()["DataSource"] = items;

            v["Type"] = "TextNameValue";
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public ListItemCollection AddTextNameValue(string title)
        {
            var t = new ListItemCollection();
            this.AddTextNameValue(title, t);
            return t;
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public ListItemCollection AddTextNameValue()
        {
            var t = new ListItemCollection();
            this.AddTextNameValue(t);
            return t;
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public ListItemCollection AddTextValue(string title)
        {
            var t = new ListItemCollection();
            this.AddTextValue(title, t);
            return t;
        }
        /// <summary>
        /// 增加字典对说明
        /// </summary>
        /// <param name="items"></param>
        public ListItemCollection AddTextValue()
        {
            var t = new ListItemCollection();
            this.AddTextValue(t);
            return t;
        }
        /// <summary>
        /// 增加一个图片
        /// </summary>
        /// <param name="src"></param>
        public void AddImage(Uri src)
        {
            WebMeta v = new WebMeta();
            v["Src"] = src.AbsoluteUri;//.ToString();
            v["Type"] = "Image";
            v["Name"] = UMC.Data.Utility.Parse62Encode(Guid.NewGuid().GetHashCode());
            this.dataSrouce.Add(v);
        }


        /// <summary>
        /// 增加密码输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddPassword(string title, string Code, bool IsDisabledMD5)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            if (IsDisabledMD5)
            {
                v["IsDisabledMD5"] = "true";
            }
            else
            {
                v["Time"] = Data.Utility.TimeSpan().ToString();
            }
            v["Type"] = "Password";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
        }
        /// <summary>
        /// 增加密码输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="defaultValue"></param>
        public WebMeta AddPassword(string title, string Code, string defaultValue)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = defaultValue;
            v["Time"] = Data.Utility.TimeSpan().ToString();
            v["Type"] = "Password";
            v["Name"] = Code;
            this.dataSrouce.Add(v);
            return v;
        }
        public WebMeta AddUI(String title, String name, string desc)
        {
            WebMeta v = new WebMeta();
            v["Title"] = title;
            v["DefaultValue"] = desc;
            v["Type"] = "UI";
            v["Name"] = name;// WebADNuke.Utils.Utility.Parse62Encode(Guid.NewGuid().GetHashCode());
            this.dataSrouce.Add(v);
            return v;

        }

        public WebMeta AddUI(String title, string desc)
        {
            return AddUI(title, UMC.Data.Utility.Parse62Encode(Guid.NewGuid().GetHashCode()), desc);
        }
        public WebMeta AddUIIcon(String icon, string title)
        {
            return AddUIIcon(icon, title, String.Empty, 0);
        }
        public WebMeta AddUIIcon(String icon, string title, string desc)
        {
            return AddUIIcon(icon, title, desc, 0);
        }
        public WebMeta AddUIIcon(String icon, string title, int color)
        {
            return AddUIIcon(icon, title, String.Empty, color);
        }
        public WebMeta AddUIIcon(String icon, string title, string desc, int color)
        {
            WebMeta v = new WebMeta();
            v["Icon"] = icon;
            if (color != 0)
            {
                if (color < 0x1000)
                {
                    v["Color"] = String.Format("#{0:x3}", color);
                }
                else
                {
                    v["Color"] = String.Format("#{0:x6}", color);
                }
            }
            if (String.IsNullOrEmpty(title) == false)
                v["Title"] = title;
            if (String.IsNullOrEmpty(desc) == false)
                v["DefaultValue"] = desc;
            v["Type"] = "UI";
            v["Name"] = UMC.Data.Utility.Parse62Encode(Guid.NewGuid().GetHashCode());
            this.dataSrouce.Add(v);
            return v;
        }

        /// <summary>
        /// 增加时间选择框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="code"></param>
        public WebMeta AddTime(string title, string code, int hour, int minute)
        {
            WebMeta v = new WebMeta();//"DataSource", ds.ToArray());
            v["Title"] = title;
            v["Type"] = "Time";
            v["DefaultValue"] = String.Format("{0:00}:{1:00}", hour, minute);
            v["Name"] = code;
            this.dataSrouce.Add(v);
            return v;

        }
        /// <summary>
        /// 增加时间选择框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="code"></param>
        public WebMeta AddTime(string title, string code, DateTime? defaultValue)
        {
            WebMeta v = new WebMeta();//"DataSource", ds.ToArray());
            v["Title"] = title;
            v["Type"] = "Time";
            if (defaultValue.HasValue)
            {
                v["DefaultValue"] = defaultValue.Value.ToString("HH:mm");
            }
            v["Name"] = code;
            this.dataSrouce.Add(v);
            return v;
        }

        /// <summary>
        /// 增加选择框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Code"></param>
        /// <param name="items"></param>
        public ListItemCollection AddSelect(string title, string code)
        {
            ListItemCollection t = new ListItemCollection();
            AddSelect(title, code, t);
            return t;
        }
        /// <summary>
        /// 增加选择框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="code"></param>
        /// <param name="items"></param>
        public void AddSelect(string title, string code, ListItemCollection items)
        {
            WebMeta v = new WebMeta();
            v.GetDictionary()["DataSource"] = items;
            v["Title"] = title;
            v["Type"] = "Select";
            v["Name"] = code;
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加复选框
        /// </summary>
        public ListItemCollection AddCheckBox(string title, string code, string defaultValue)
        {
            ListItemCollection t = new ListItemCollection();
            AddCheckBox(title, code, t, defaultValue);
            return t;
        }
        /// <summary>
        /// 增加复选框
        /// </summary>
        public ListItemCollection AddCheckBox(string title, string code)
        {
            ListItemCollection t = new ListItemCollection();
            AddCheckBox(title, code, t);
            return t;
        }
        public void AddCheckBox(string title, string code, ListItemCollection items, string defaultValue)
        {

            WebMeta v = new WebMeta();
            v.GetDictionary()["DataSource"] = items;
            v["Title"] = title;
            if (String.IsNullOrEmpty(defaultValue) == false)
            {
                v["DefaultValue"] = defaultValue;
            }
            v["Type"] = "CheckboxGroup";
            v["Name"] = code;
            this.dataSrouce.Add(v);
        }
        /// <summary>
        /// 增加复选框
        /// </summary>
        public void AddCheckBox(string title, string code, ListItemCollection items)
        {
            AddCheckBox(title, code, items, null);
        }
        /// <summary>
        /// 增加单选择框
        /// </summary>
        public ListItemCollection AddRadio(string title, string code)
        {
            ListItemCollection t = new ListItemCollection();
            AddRadio(title, code, t);
            return t;
        }
        /// <summary>
        /// 增加单选择框
        /// </summary>
        public void AddRadio(string title, string code, ListItemCollection items)
        {
            WebMeta v = new WebMeta();
            v.GetDictionary()["DataSource"] = items;
            v["Title"] = title;
            v["Type"] = "RadioGroup";
            v["Name"] = code;
            this.dataSrouce.Add(v);

        }
        /// <summary>
        /// 设置提交按钮名称，并设置当前位置
        /// </summary>
        /// <param name="btnName"></param>
        public void Submit(String btnName)
        {
            this.Config.Put("submit", btnName);
            this.dataSrouce[this.dataSrouce.Count - 1].Put("Submit", "YES");
        }
        /// <summary>
        /// 事件提交参数配置
        /// </summary>
        /// <param name="btnName">提交按钮名称</param>
        /// <param name="model">提交的模块</param>
        /// <param name="cmd">提交的指令</param>
        /// <param name="param">参数</param>
        public void Submit(String btnName, string model, string cmd, WebMeta param)
        {
            var p = new WebMeta();
            if (param != null && param.Count > 0)
            {
                p.Set("send", param);
            }
            p["model"] = model;
            p["cmd"] = cmd;
            if (String.IsNullOrEmpty(btnName) == false)
            {
                p["text"] = btnName;
            }

            this.Config.Set("submit", p);
            this.dataSrouce[this.dataSrouce.Count - 1].Put("Submit", "YES");

        }
        WebMeta submit;
        /// <summary>
        /// 事件提交参数配置
        /// </summary>
        /// <param name="btnName">提交按钮名称</param>
        /// <param name="model">提交的模块</param>
        /// <param name="cmd">提交的指令</param>
        /// <param name="colseEvent">关闭对话框的事件</param>
        public void Submit(String btnName, string model, string cmd, params string[] colseEvent)
        {
            var p = new WebMeta();

            p["model"] = model;
            p["cmd"] = cmd;
            if (String.IsNullOrEmpty(btnName) == false)
            {
                p["text"] = btnName;
            }
            if (colseEvent.Length > 0)
            {
                this.Config.Put("CloseEvent", String.Join(",", colseEvent));
            }
            this.Config.Set("submit", p);

        }
        /// <summary>
        /// 不使用提交按钮
        /// </summary>
        public void HideSubmit()
        {
            this.Config.Put("submit", false);
        }
        /// <summary>
        /// 在最后一个控件下添加提交按钮
        /// </summary>
        public void Submit()
        {
            this.dataSrouce[this.dataSrouce.Count - 1].Put("Submit", "YES");
        }
        /// <summary>
        /// 增加验收码输入框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="code"></param>
        /// <param name="placeholder"></param>
        /// <returns></returns>
        public WebMeta AddVerify(String title, String code, String placeholder)
        {

            WebMeta v = new WebMeta();

            v["Title"] = title;
            v["Type"] = "Verify";
            v["Name"] = code;
            v["placeholder"] = placeholder;
            this.dataSrouce.Add(v);
            return v;

        }
        /// <summary>
        /// 事件提交方式参数配置，
        /// </summary>
        /// <param name="btnName">提交按钮名称</param>
        /// <param name="request">提交的当前指令有参数</param>
        /// <param name="colseEvent">对话框关闭的事件</param>
        public void Submit(string btnName, WebRequest request, params string[] colseEvent)
        {
            if (colseEvent.Length > 0)
            {
                this.Config.Put("CloseEvent", String.Join(",", colseEvent));
            }
            var pa = new WebMeta(request.Arguments.GetDictionary());

            submit = new WebMeta("model", request.Model, "cmd", request.Command, "text", btnName).Put("send", pa);
            Submit(btnName);
        }
        protected override void Initialization(WebContext context)
        {
            if (submit != null)
            {
                var send = submit.GetDictionary()["send"] as WebMeta;
                send.Put(UIDialog.KEY_DIALOG_ID, this.AsyncId);
                //submit._send !=
                this.Config.Put("submit", submit);
            }
            this.Config.Put("DataSource", dataSrouce);
        }

        public List<WebMeta> DataSource
        {
            get
            {
                return dataSrouce;
            }

        }

    }
}
