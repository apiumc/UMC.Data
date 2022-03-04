using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 选择对话框
    /// </summary>
    public class UISelectDialog : UIDialog
    {
        ListItemCollection _nSource = new ListItemCollection();



        /// <summary>
        /// 文本对话框选择配置
        /// </summary>
        public ListItemCollection Options
        {
            get
            {
                return _nSource;
            }
        }

        //public int Size
        //{
        //    get;
        //    set;
        //}
        protected override void Initialization(WebContext context)
        {
            //var ds = new List<POSMeta>();
            //for (var i = 0; i < _nSource.Count; i++)
            //{
            //    var d = new POSMeta();
            //    var item = _nSource[i];
            //    d["Value"] = item.Value;
            //    d["Text"] = item.Text;
            //    if (item.Disabled) d["Disabled"] = "true";
            //    if (item.Selected) d["Selected"] = "true";
            //    ds.Add(d);
            //}
            //if (Size > 1)
            //{
            //    this.Config["Size"] = this.Size.ToString();
            //}
            this.Config.GetDictionary()["DataSource"] = _nSource;
            //this.Config.Set("DataSource", ds.ToArray());//= sb.ToString();
        }
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected override string DialogType
        {
            get { return "Select"; }
        }


    }
    /// <summary>
    /// 单选框
    /// </summary>
    public class UIRadioDialog : UISelectDialog
    {
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected override string DialogType
        {
            get
            {
                return "RadioGroup";
            }
        }
    }
    /// <summary>
    /// 复选框 
    /// </summary>
    public class UICheckboxDialog : UISelectDialog
    {
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected override string DialogType
        {
            get
            {
                return "CheckboxGroup";
            }
        }
        protected override void Initialization(Web.WebContext context)
        {
            if (String.IsNullOrEmpty(this.DefaultValue) == false)
            {
                this.Config["DefaultValue"] = this.DefaultValue;
            }
            base.Initialization(context);
        }
    }
}
