using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 日期对话框
    /// </summary>
    public class UIDateDialog : UIDialog
    {
        public UIDateDialog(DateTime defaultValue)
        {
            this.Config["DefaultValue"] = defaultValue.ToString("yyyy-MM-dd");
        }
        public UIDateDialog() { }


        protected override string DialogType
        {
            get { return "Date"; }
        }

    }
}
