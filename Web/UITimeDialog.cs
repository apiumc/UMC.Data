using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 时间对话框
    /// </summary>
    public class UITimeDialog : UIDialog
    {
        public UITimeDialog(int hour, int minute)
        {
            this.Config["DefaultValue"] = String.Format("{0}:{1}", hour, minute);
        }
        public UITimeDialog() { }


        protected override string DialogType
        {
            get { return "Time"; }
        }

    }
}
