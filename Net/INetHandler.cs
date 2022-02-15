using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Net
{
    public interface INetHandler
    {
        /// <summary>
        /// 进行
        /// </summary>
        void ProcessRequest(NetContext context);
    }

}
