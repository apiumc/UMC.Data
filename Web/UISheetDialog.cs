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
    public class UISheetDialog : UIDialog
    {
        List<UIClick> _nSource = new List<UIClick>();



        /// <summary>
        /// 文本对话框选择配置
        /// </summary>
        public List<UIClick> Options
        {
            get
            {
                return _nSource;
            }
        }
        protected override void Initialization()
        {
            this.Config.GetDictionary()["DataSource"] = _nSource;
        }
        /// <summary>
        /// 对话框类型
        /// </summary>
        protected override string DialogType
        {
            get { return "Select"; }
        }


    }
}
