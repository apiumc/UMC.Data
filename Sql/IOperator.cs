using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 数据库操作运算符
    /// </summary>
    public interface IOperator<T> where T : class
    {
        /// <summary>
        /// 实体不等于 &gt;&gt;
        /// </summary>
        /// <param name="field">非空属性实体</param>
        /// <returns></returns>
        IWhere<T> Unequal(T field);
        /// <summary>
        /// 实体等于 =
        /// </summary>
        /// <param name="field">非空属性实体</param>
        IWhere<T> Equal(T field);
        /// <summary>
        /// 实体大于 &gt;
        /// </summary>
        /// <param name="field">非空属性实体</param>
        IWhere<T> Greater(T field);
        /// <summary>
        /// 实体小于&lt;
        /// </summary>
        /// <param name="field">非空属性实体</param>
        IWhere<T> Smaller(T field);
        /// <summary>
        /// 实体大于等于 &gt;=
        /// </summary>
        /// <param name="field">非空属性实体</param>
        IWhere<T> GreaterEqual(T field);
        /// <summary>
        /// 实体小于等于 &lt;=
        /// </summary>
        /// <param name="field">非空属性实体</param>
        IWhere<T> SmallerEqual(T field);
        /// <summary>
        /// 不等于&lt;&gt;
        /// </summary>
        IWhere<T> Unequal(string field, object value);
        /// <summary>
        /// 等于 =
        /// </summary>
        IWhere<T> Equal(string field, object value);
        /// <summary>
        /// 大于 &gt;
        /// </summary>
        IWhere<T> Greater(string field, object value);
        /// <summary>
        /// 小于&lt;
        /// </summary>
        IWhere<T> Smaller(string field, object value);
        /// <summary>
        /// 大于等于 &gt;=
        /// </summary>
        IWhere<T> GreaterEqual(string field, object value);
        /// <summary>
        /// 小于等于 &lt;=
        /// </summary>
        IWhere<T> SmallerEqual(string field, object value);
        /// <summary>
        /// 不等于
        /// </summary>
        IWhere<T> NotLike(string field, string value);
        /// <summary>
        /// like
        /// </summary>
        IWhere<T> Like(string field, string value);
        /// <summary>
        /// like
        /// </summary>
        IWhere<T> Like(T field, bool schar);
        /// <summary>
        /// like
        /// </summary>
        IWhere<T> Like(T field);
        /// <summary>
        /// In
        /// </summary>
        IWhere<T> In(string field, params object[] values);
        /// <summary>
        /// In
        /// </summary>
        /// <param name="field">只能一个非空字段的值</param>
        IWhere<T> In(T field, params object[] values);
        /// <summary>
        /// In
        /// </summary>
        IWhere<T> In(string field, Script script);
        /// <summary>
        /// In
        /// </summary>
        IWhere<T> In(T field, Script script);
        /// <summary>
        /// Not In
        /// </summary>
        IWhere<T> NotIn(string field, params object[] values);
        /// <summary>
        /// Not In 
        /// </summary>
        /// <param name="field">只能一个非空字段的值</param>
        IWhere<T> NotIn(T field, params object[] values);
        /// <summary>
        /// Not In
        /// </summary>
        IWhere<T> NotIn(T field, Script script);
        /// <summary>
        /// Not In
        /// </summary>
        IWhere<T> NotIn(string field, Script script);
        /// <summary>
        /// 创建带小括号 SQL WHERE条件，例如 ：(field1=1)
        /// </summary>
        /// <returns></returns>
        IWhere<T> Contains();
    }
    /// <summary>
    /// 数据库操作运算符
    /// </summary>
    //interface IOperator
    //{
    //    /// <summary>
    //    /// 不等于&lt;&gt;
    //    /// </summary>
    //    Conditions Unequal(string field, object value);
    //    /// <summary>
    //    /// 等于 =
    //    /// </summary>
    //    Conditions Equal(string field, object value);
    //    /// <summary>
    //    /// 大于 &gt;
    //    /// </summary>
    //    Conditions Greater(string field, object value);
    //    /// <summary>
    //    /// 小于&lt;
    //    /// </summary>
    //    Conditions Smaller(string field, object value);
    //    /// <summary>
    //    /// 大于等于 &gt;=
    //    /// </summary>
    //    Conditions GreaterEqual(string field, object value);
    //    /// <summary>
    //    /// 小于等于 &lt;=
    //    /// </summary>
    //    Conditions SmallerEqual(string field, object value);
    //    /// <summary>
    //    /// like
    //    /// </summary>
    //    Conditions NotLike(string field, string value);
    //    /// <summary>
    //    /// like
    //    /// </summary>
    //    Conditions Like(string field, string value);
    //    /// <summary>
    //    /// In
    //    /// </summary>
    //    Conditions In(string field, params object[] values);
    //    /// <summary>
    //    /// Not In
    //    /// </summary>
    //    Conditions NotIn(string field, params object[] values);

    //    /// <summary>
    //    /// 创建带小括号 条件，例如 ：(field1=1 AND field2=2)
    //    /// </summary>
    //    //IWhere Contains();
    //}
}
