using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 实体更新适配器
    /// </summary>
    public interface IGrouper<T> : IScript where T : class
    {
        T Single();
        /// <summary>
        /// 查询分组
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable Query();
        /// <summary>
        /// 查询分组
        /// </summary>
        //void Query(DataReader reader);
        /// <summary>
        /// 查询分组
        /// </summary>
        void Query(DataReader<T> reader);
        /// <summary>
        /// 排序
        /// </summary>
        IGroupOrder<T> Order { get; }

        IGrouper<T> Count(String asName);
        /// <summary>
        /// 求记录数,对应的字段为"G"+(i+1)，i为统计次数,例如：e.GroupBy("field'}).Count(),则Count的列名为"G1"
        /// </summary>
        /// <returns></returns>
        IGrouper<T> Count(T field);
        /// <summary>
        /// 求记录数,对应的字段为"G"+(i+1)，i为统计次数,例如：e.GroupBy("field'}).Count(),则Count的列名为"G1"
        /// </summary>
        /// <returns></returns>
        IGrouper<T> Count();
        /// <summary>
        /// 求和,对应的字段为"G"+(i+1)，i为统计次数,例如：e.GroupBy("field'}).Sum(),则Sum的列名为"G1"
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Sum(string field);
        IGrouper<T> Sum(string field, string asName);
        /// <summary>
        /// 求平均,对应的字段为"G"+(i+1)，i为统计次数,例如：e.GroupBy("field'}).Sum(field),则Sum的列名为"G1"
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Avg(string field);
        IGrouper<T> Avg(string field, string asName);
        /// <summary>
        /// 求最大值,对应的字段为"G"+(i+1)，i为统计次数,例如：e.GroupBy("field'}).Max(field),则Max的列名为"G1"
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Max(string field);
        IGrouper<T> Max(string field, string asName);
        /// <summary>
        /// 求最小值,,对应的字段为"G"+(i+1)，i为统计次数,例如：e.GroupBy("field'}).Min(field),则Min的列名为"G1"
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Min(string field);
        IGrouper<T> Min(string field, string asName);
        /// <summary>
        /// 求和,对应的字段值为实体非空字段
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Sum(T field);
        /// <summary>
        /// 求平均,对应的字段值为实体非空字段
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Avg(T field);
        /// <summary>
        /// 求最大值,对应的字段值为实体非空字段
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Max(T field);
        /// <summary>
        /// 求最小值,对应的字段值为实体非空字段
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        IGrouper<T> Min(T field);
        ///// <summary>
        ///// 求和,对应的字段值为实体非空字段
        ///// </summary>
        ///// <param name="field">字段</param>
        ///// <returns></returns>
        //IGrouper<T> Sum(T field, T asField);
        ///// <summary>
        ///// 求平均,对应的字段值为实体非空字段
        ///// </summary>
        ///// <param name="field">字段</param>
        ///// <returns></returns>
        //IGrouper<T> Avg(T field, T asField);
        ///// <summary>
        ///// 求最大值,对应的字段值为实体非空字段
        ///// </summary>
        ///// <param name="field">字段</param>
        ///// <returns></returns>
        //IGrouper<T> Max(T field, T asField);
        ///// <summary>
        ///// 求最小值,对应的字段值为实体非空字段
        ///// </summary>
        ///// <param name="field">字段</param>
        ///// <returns></returns>
        //IGrouper<T> Min(T field, T asField);
    }
}
