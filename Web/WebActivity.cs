using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UMC.Web
{
    /// <summary>
    /// 处理
    /// </summary>
    public abstract class WebActivity : WebHandler
    {
        class POSFinishActivity : WebActivity
        {
            public override void ProcessActivity(WebRequest request, WebResponse response)
            {
                //  throw new NotImplementedException();
            }
        }
        /// <summary>
        /// 终止流程的活动
        /// </summary>
        public static readonly WebActivity Empty = new POSFinishActivity();
        /// <summary>
        /// 活动Id
        /// </summary>
        public virtual string Id
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// 活动节点处理方法
        /// </summary>
        /// <param name="request">请求</param>
        /// <param name="response">响应</param>
        /// <returns></returns>
        public abstract void ProcessActivity(WebRequest request, WebResponse response);

    }
}