using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{



    /// <summary>
    /// 表格对话框 
    /// </summary>  
    public abstract class UIGridDialog : UIDialog
    {
        public class Header
        {
            System.Collections.Hashtable headers = new System.Collections.Hashtable();
            List<System.Collections.Hashtable> fields = new List<System.Collections.Hashtable>();
            public Header(int pageSize)
            {
                headers["type"] = "grid";
                headers["pageSize"] = pageSize;
            }
            /// <summary>
            /// 如果valueField为Empty，则type将是editor
            /// </summary>
            /// <param name="valueField">对话框返回字段</param>
            /// <param name="pageSize">分页长度</param>
            public Header(string valueField, int pageSize)
            {
                headers["type"] = "dialog";

                headers["pageSize"] = pageSize;
                if (String.IsNullOrEmpty(valueField))
                {
                    headers["type"] = "grid";
                }
                else
                {
                    headers["ValueField"] = valueField;
                }
            }
            protected System.Collections.Hashtable GetField(string fieldName)
            {
                var field = fields.Find(f => (String)f["Name"] == fieldName);
                if (field == null)
                {
                    field = new System.Collections.Hashtable();
                    field["Name"] = fieldName;
                    field["type"] = "string";
                    fields.Add(field);
                }
                return field;
            }

            public void AddField(string field, string name)
            {
                var f = GetField(field);
                f["config"] = UMC.Data.ConfigurationManager.ParseDictionary(name);
            }
            public Header PutField(string fieldName, string config)
            {
                AddField(fieldName, config);
                return this;
            }
            public System.Collections.Hashtable GetHeader()
            {
                headers["fields"] = fields;
                return headers;
            }
        }
        protected UIGridDialog()
        {
            this.IsReturnValue = true;
        }

        /// <summary>
        /// 是否有返回值
        /// </summary>
        public bool IsReturnValue
        {
            get;
            set;
        }
        protected abstract Hashtable GetHeader();
        protected abstract Hashtable GetData(IDictionary paramsKey);

        public bool AutoSearch
        {
            get;
            set;
        }
        /// <summary>
        /// 类型
        /// </summary>
        protected override string DialogType
        {
            get { return "Grid"; }
        }
        /// <summary>
        /// 搜索
        /// </summary>
        public bool IsSearch
        {
            get;
            set;
        }
        /// <summary>
        /// Keywork搜索提示
        /// </summary>
        public string Keyword
        {
            get;
            set;
        }
        protected void Search(string model, string cmd, WebMeta param, string submodel, string subcmd)
        {
            this.Search(model, cmd, param, submodel, subcmd, null);
        }
        protected void Search(string model, string cmd, WebMeta param, string submodel, string subcmd, WebMeta send)
        {
            var p = new WebMeta();
            if (param != null)
            {
                p.Set("params", param);
            }
            p["model"] = model;
            p["cmd"] = cmd;

            var sub = new WebMeta();

            sub["model"] = submodel;
            sub["cmd"] = subcmd;
            if (send != null)
            {
                sub.Put("send", send);
            }
            p.Set("submit", sub);
            this.Config.Set("search", p);

        }

        public void Menu(string text, string model, string cmd, string value)
        {
            this.Menu(new UIClick(value) { Text = text }.Send(model, cmd));
        }
        public void Menu(params UIClick[] menus)
        {
            this.Config.Put("menu", menus);
        }
        public void Menu(string text, string model, string cmd, WebMeta param)
        {
            this.Menu(new UIClick(param) { Text = text }.Send(model, cmd));
        }
        public bool IsPage
        {
            get;
            set;
        }

        public WebMeta ValueField
        {
            get; set;
        }
        protected override void Initialization(WebContext context)
        {
            //var context = WebContext.Current;
            var request = context.Request;
            var response = context.Response;
            if (String.IsNullOrEmpty(this.AsyncId) == false && request.Items.ContainsKey(this.AsyncId))
            {
                WebMeta meta = request.SendValues;
                if (meta != null)
                {
                    response.Redirect(this.GetData(meta.GetDictionary()));
                }
                else
                {
                    var paramKey = request.Headers.GetDictionary()[EventType.Dialog] as Hashtable ?? new Hashtable();
                    response.Redirect(this.GetData(paramKey));
                }

            }
            else
            {
                var p = GetHeader();
                if (this.ValueField != null)
                {
                    p["ValueField"] = this.ValueField;
                }
                if (IsPage)
                {
                    WebMeta meta = request.SendValues;
                    if (meta != null && meta.Count > 0)
                    {
                        response.Redirect(this.GetData(meta.GetDictionary()));
                    }
                    else if (request.Arguments.Count == 0)
                    {
                        Search(request.Model, request.Command, null, request.Model, request.Command, null);
                    }
                    else
                    {
                        var pa = new WebMeta(new Hashtable(request.Arguments.GetDictionary()));
                        Search(request.Model, request.Command, pa, request.Model, request.Command, pa);
                        if (this.ValueField == null)
                        {
                            p["send"] = this.AsyncId;
                        }

                    }
                }
                if (String.IsNullOrEmpty(this.AsyncId) == false)
                {
                    request.Items[this.AsyncId] = "Header";
                }
                p["title"] = this.Title;
                if (IsAsyncData)
                {
                    this.Config.Set("Data", this.GetData(new Hashtable()));
                }
                else if (this.IsSearch)
                {

                    p["search"] = this.Keyword ?? "搜索";
                }

                if (this.IsReturnValue)
                {
                    p["type"] = "dialog";
                }
                else
                {
                    p["type"] = "grid";
                }
                if (this.AutoSearch)
                {
                    p["auto"] = "true";
                }
                this.Config.Set("Header", p);
            }
            base.Initialization(context);
        }
    }
    //public abstract class UITableDialog : POSDialog
    //{
    //    protected override string DialogType
    //    {
    //        get { return "Table"; }
    //    }
    //    public String Cell
    //    {
    //        get; set;
    //    }
    //    public String ValueKey
    //    {
    //        get; set;
    //    }
    //    POSMeta _fromat = new POSMeta();
    //    UI.UIStyle _style = new UI.UIStyle();
    //    public POSMeta Format
    //    {
    //        get
    //        {
    //            return _fromat;
    //        }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public UI.UIStyle Style
    //    {
    //        get
    //        {
    //            return _style;
    //        }
    //    }
    //    public void Menu(string text, string model, string cmd, string value)
    //    {
    //        this.Menu(CreateMenu(text, model, cmd, value));
    //    }
    //    public void Menu(params POSMeta[] menus)
    //    {
    //        this.Config.Set("menu", menus);
    //    }
    //    protected abstract Hashtable GetData(IDictionary paramsKey);


    //    protected override void Initialization()
    //    {
    //        var context = POSContext.Current;
    //        var request = context.Request;
    //        var response = context.Response;
    //        if (String.IsNullOrEmpty(this.AsyncId) == false && request.Items.ContainsKey(this.AsyncId))
    //        {
    //            POSMeta meta = request.SendValues;
    //            if (meta != null)
    //            {
    //                response.Redirect(this.GetData(meta.GetDictionary()));
    //            }
    //            else
    //            {
    //                var paramKey = request.Headers.GetDictionary()[EventType.Dialog] as Hashtable ?? new Hashtable();
    //                response.Redirect(this.GetData(paramKey));
    //            }

    //        }
    //        else
    //        {

    //            var pa = new POSMeta(new Hashtable(request.Arguments.GetDictionary()));
    //            var database = new POSMeta();
    //            if (IsAsyncData)
    //            {
    //                database.Put("data", this.GetData(new Hashtable()));

    //            }
    //            else
    //            {
    //                database.Put("model", request.Model, "cmd", request.Command);
    //                if (pa.Count > 0)
    //                {
    //                    database.Put("search", pa);
    //                }
    //            }
    //            database.Put("cell", this.Cell ?? "Cell")
    //              .Put("style", this.Style)
    //              .Put("format", this.Format)
    //              .Put("click", new UI.UIClick(new POSMeta(new Hashtable(pa.GetDictionary())).Put(this.AsyncId, this.ValueKey ?? this.AsyncId))
    //              {
    //                  Command = request.Command,
    //                  Model = request.Model
    //              });
    //            this.Config.Put("type", "DataSource").Put("title", this.Title)
    //               .Put("DataSource", new POSMeta[] { database });

    //            context.Send(this.Config, true);



    //        }
    //    }
    //}
    //public abstract class UIPageDialog : POSDialog
    //{

    //    protected override string DialogType
    //    {
    //        get { return "Page"; }
    //    }
    //    protected abstract UI.UISection Section(IDictionary paramsKey);


    //    protected override void Initialization()
    //    {
    //        var context = POSContext.Current;
    //        var request = context.Request;
    //        var response = context.Response;
    //        if (String.IsNullOrEmpty(this.AsyncId) == false && request.Items.ContainsKey(this.AsyncId))
    //        {
    //            POSMeta meta = request.SendValues;
    //            if (meta != null)
    //            {
    //                response.Redirect(this.Section(meta.GetDictionary()));
    //            }
    //            else
    //            {
    //                var paramKey = request.Headers.GetDictionary()[EventType.Dialog] as Hashtable ?? new Hashtable();
    //                response.Redirect(this.Section(paramKey));
    //            }

    //        }
    //        else
    //        {

    //            var pa = new POSMeta(new Hashtable(request.Arguments.GetDictionary()));
    //            this.Config.Put("type", "Page").Put("title", this.Title).Put("model", request.Model, "cmd", request.Command);
    //            if (pa.Count > 0)
    //                this.Config.Put("search", pa);
    //            context.Send(this.Config, true);
    //        }
    //    }
    //}

}
