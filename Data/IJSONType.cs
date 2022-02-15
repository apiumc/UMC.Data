using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data
{
    /// <summary>
    /// 获取属性类型
    /// </summary>
    public interface IJSONType
    {
        /// <summary>
        /// 获取属性类型
        /// </summary>
        /// <param name="prototypeName">属性名</param>
        /// <returns></returns>
        Type GetType(string prototypeName);
    }
}
