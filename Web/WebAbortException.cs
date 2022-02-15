using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 终止后面的执行
    /// </summary>
    public sealed class WebAbortException : Exception
    {
    }
    class EventType
    {
        /// <summary>
        /// 对话框
        /// </summary>
        public const string AsyncDialog = "AsyncDialog";

        public const string Complete = "Complete";
        /// <summary>
        /// 对话框
        /// </summary>
        public const string Dialog = "Dialog";
        /// <summary>
        /// 单据交易完成
        /// </summary>
        public const string FinishAfter = "FinishAfter";
        /// <summary>
        /// 单据交易完成 
        /// </summary>
        public const string Finish = "Finish";

    }

}
