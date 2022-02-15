using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 实体类型的Where查询接口：注意他有父子级概念
    /// </summary>

    public interface IWhere<T> where T : class
    {
        /// <summary>
        /// 设置或获取设置的字段条件值
        /// </summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        object this[string name] { get; set; }
        /// <summary>
        /// 清空所有的运算配置
        /// </summary>
        //IWhere Reset();
        /// <summary>
        /// 移除运算配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int Remove(string name);

        /// <summary>
        /// 当前参数数量
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// 配置Sql表达式的查询条件
        /// </summary>
        /// <param name="expression">sql表达式</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        //IWhere Or(string expression, params Object[] paramers);

        /// <summary>
        /// 配置Sql表达式的查询条件
        /// </summary>
        /// <param name="expression">sql表达式</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        //IWhere And(string expression, params Object[] paramers);
        /// <summary>
        ///获取 Or运算
        /// </summary>
        /// <returns></returns>
        //IOperator Or();
        ///// <summary>
        /////获取 And运算
        ///// </summary>
        ///// <returns></returns>
        //IOperator And();

        /// <summary>
        /// 创建带小括号 SQL WHERE条件，例如 ：(field1=1 AND field2=2)
        /// </summary>
        //IWhere Contains();
        //}
        ///// <summary>
        ///// 实现实体查询
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        //public interface IWhere<T> : IWhere where T : class
        //{
        IObjectEntity<T> Entities
        {
            get;
        }

        /// <summary>
        /// 重新所有的运算配置
        /// </summary>
        IWhere<T> Reset();

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="field">实体条件</param>
        /// <returns></returns>
        IWhere<T> Remove(T field);

        /// <summary>
        /// 替换参数
        /// </summary>
        /// <param name="field">实体条件</param>
        /// <returns></returns>
        IWhere<T> Replace(T field);

        /// <summary>
        /// 配置Sql表达式的查询条件
        /// </summary>
        /// <param name="expression">sql表达式</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        IWhere<T> Or(string expression, params Object[] paramers);


        /// <summary>
        /// 配置Sql表达式的查询条件
        /// </summary>
        /// <param name="expression">sql表达式</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        IWhere<T> And(string expression, params Object[] paramers);
        /// <summary>
        ///获取 Or运算
        /// </summary>
        /// <returns></returns>
        IOperator<T> Or();
        /// <summary>
        ///获取 And运算
        /// </summary>
        /// <returns></returns>
        IOperator<T> And();

        /// <summary>
        ///实体条件Or
        /// </summary>
        /// <returns></returns>
        IWhere<T> Or(T field);
        /// <summary>
        ///实体条件And
        /// </summary>
        /// <returns></returns>
        IWhere<T> And(T field);

        /// <summary>
        /// 创建带小括号 SQL WHERE条件，例如 ：(field1=1 AND field2=2)
        /// </summary>
        /// <returns></returns>
        IWhere<T> Contains();
    }
}
