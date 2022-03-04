using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 确定提示框
    /// </summary>
    public class UIConfirmDialog : UIDialog
    {
        public UIConfirmDialog(string text)
        {
            this.Title = "提示";
            this.Config["Text"] = text;
        }

        public UIConfirmDialog(string text, string defaultValue)
        {
            this.Config["Text"] = text;
            this.DefaultValue = defaultValue;
        }

        protected override string DialogType
        {
            get { return "Confirm"; }
        }
        protected override void Initialization(WebContext context)
        {
            this.Config["DefaultValue"] = this.DefaultValue ?? "YES";
            base.Initialization(context);
        }
    }
}
