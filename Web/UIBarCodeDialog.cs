using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 条码对话框，如果客户端不支持扫描条码，则显示文本对话框
    /// </summary>
    public class UIBarCodeDialog : UIDialog
    { 
        /// <summary>
        /// 文本对话框
        /// </summary>
        public UIBarCodeDialog() { }
        /// <summary>
        /// 文本对话框
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        public UIBarCodeDialog(string defaultValue)
        {

            this.Config["DefaultValue"] = defaultValue;
        }
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected override string DialogType
        {
            get { return "BarCode"; }
        }
    }
}
