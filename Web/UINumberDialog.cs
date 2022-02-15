using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 数字对话框
    /// </summary>
    public class UINumberDialog : UIDialog
    {
        public UINumberDialog(decimal? defaultValue)
        {
            this.Config["DefaultValue"] = defaultValue.ToString();
        }
        public UINumberDialog(string defaultValue)
        {
            this.Config["DefaultValue"] = defaultValue;
        }
        public UINumberDialog() { }


        protected override string DialogType
        {
            get { return "Number"; }
        }

    }
}
