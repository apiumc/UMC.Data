using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 文本对话框
    /// </summary>
    public class UITextDialog : UIDialog
    {
        /// <summary>
        /// 文本对话框
        /// </summary>
        public UITextDialog() { }
        /// <summary>
        /// 文本对话框
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        public UITextDialog(string defaultValue)
        {

            this.Config["DefaultValue"] = defaultValue;
        }
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected override string DialogType
        {
            get { return "Text"; }
        }
    }
}
