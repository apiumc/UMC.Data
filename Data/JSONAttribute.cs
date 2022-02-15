using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data
{

    /// <summary>
    /// 表示类或属性可以用JSON序列化
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class JSONAttribute : System.Attribute
    {
    }
}
