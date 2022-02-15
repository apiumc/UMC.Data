using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UMC.Web
{
    /// <summary>
    /// 客户端下一步请求
    /// </summary>
    class ClientRedirect
    {
        public string Model
        {
            get;
            set;
        }
        public string Command
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
    }
}