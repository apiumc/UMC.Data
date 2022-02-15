using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 运算词
    /// </summary>
    enum DbOperator
    {
        /// <summary>
        /// 不等于&lt;&gt;
        /// </summary>
        Unequal = -1,
        /// <summary>
        /// 等于 =
        /// </summary>
        Equal = 1,
        /// <summary>
        /// 大于 &gt;
        /// </summary>
        Greater = 2,
        /// <summary>
        /// 小于&lt;
        /// </summary>
        Smaller = 3,
        /// <summary>
        /// 大于等于 &gt;=
        /// </summary>
        GreaterEqual = 4,
        /// <summary>
        /// 小于等于 &lt;=
        /// </summary>
        SmallerEqual = 5,
        /// <summary>
        /// like
        /// </summary>
        Like = 6,
        /// <summary>
        /// like
        /// </summary>
        NotLike = 9,
        /// <summary>
        /// 表达式
        /// </summary>
        Expression = 0,
        In = 7,
        NotIn = 8

    }
}
