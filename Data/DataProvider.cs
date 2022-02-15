using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data
{
    /// <summary>
    /// 引用配置节
    /// </summary>
    public abstract class DataProvider
    {
        /// <summary>
        /// 配置的节点
        /// </summary>
        public UMC.Data.Provider Provider
        {
            internal set;
            get;
        }
    }
}
