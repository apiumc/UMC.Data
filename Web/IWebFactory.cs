using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UMC.Web
{
    /// <summary>
    /// 线路Factory
    /// </summary>
    public interface IWebFactory
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="context"></param>
        void OnInit(WebContext context);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode">请求的模式</param> 
        /// <returns></returns>
        WebFlow GetFlowHandler(string mode);
    }
}