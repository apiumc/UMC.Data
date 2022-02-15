using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// POS路线
    /// </summary>
    public abstract class WebFlow : WebHandler
    {
        class FinishFlow : WebFlow
        {
            public override WebActivity GetFirstActivity()
            {
                return WebActivity.Empty;
            }
        }
        public static readonly WebFlow Empty = new FinishFlow();

        /// <summary>
        /// 每一次获取Activity
        /// </summary>
        /// <returns></returns>
        public abstract WebActivity GetFirstActivity();
        /// <summary>
        /// 下一次的获取的Activity
        /// </summary>
        /// <param name="ActivityId">当前的ActivityId</param>
        /// <returns></returns>
        public virtual WebActivity GetNextActivity(string ActivityId)
        {
            return WebActivity.Empty;
        }
    }
}
