using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 实体操作
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IObjectEntity<T> : IScript where T : class
    {
        /// <summary>
        /// 创建查询脚本
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        Script Script(T field);
        /// <summary>
        /// 创建查询脚本
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        Script Script(string field);
        /// <summary>
        /// 分组查询
        /// </summary>
        /// <param name="fields">分组字段名</param>
        /// <returns></returns>
        IGrouper<T> GroupBy(params string[] fields);
        /// <summary>
        /// 分组查询
        /// </summary>
        /// <param name="field">分组字段实体</param>
        /// <returns></returns>
        IGrouper<T> GroupBy(T field);
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        int Insert(params T[] items);
        /// <summary>
        /// 更新实体，如果字段“fields”的长度为0，则采用非空属性值规则更新对应的字段，否则更新指定的的字段
        /// </summary>
        /// <param name="item">实体</param>
        /// <returns>返回受影响的行数</returns>
        int Update(T item);
        /// <summary>
        /// 更新实体，如果字段“fields”的长度为0，则采用非空属性值规则更新对应的字段，否则更新指定的的字段
        /// </summary>
        /// <param name="format">更新值格式：其中{0}表示字段，{1}表示参数值</param>
        /// <param name="field">实体</param>
        /// <returns>返回受影响的行数</returns>
        int Update(string format, T field);
        /// <summary>
        /// 更新实体，第一个field按format格式更新，field2按标准实体更新
        /// </summary>
        /// <param name="format">更新值格式：其中{0}表示字段，{1}表示参数值</param>
        /// <param name="field">格式更新的字段</param>
        /// <param name="field2">实体更新的字段</param>
        /// <returns></returns>
        int Update(string format, T field, T field2);
        /// <summary>
        /// 更新实体，field按{0}+{1}格式更新，field2按标准实体更新
        /// </summary>
        /// <param name="field"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        int Update(T field, T field2);
        /// <summary>
        /// 采用字典更新实体
        /// </summary>
        /// <param name="fieldValues">字段字典对</param>
        /// <returns></returns>
        int Update(System.Collections.IDictionary fieldValues);
        /// <summary>
        /// 采用字典更新实体
        /// </summary>
        /// <param name="format">更新值格式：其中{0}表示字段，{1}表示参数值</param>
        /// <param name="fieldValues">字段字典对</param>
        /// <returns></returns>
        int Update(string format, System.Collections.IDictionary fieldValues);
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        int Delete();
        /// <summary>
        /// 排序
        /// </summary>
        IOrder<T> Order { get; }
        /// <summary>
        /// 查询一个字段，如果是“*”，必返回DataRow[]数据
        /// </summary>
        /// <returns></returns>
        object[] Query(string field);

        /// <summary>
        /// 增加查询列
        /// </summary>
        void AddField(string field, string name);


        /// <summary>
        /// 自定义处理一个字段查询的只读结果集
        /// </summary>
        void Query(string field, DataReader dr);

        void Distinct(T field, DataReader<T> dr);
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        void Query(DataReader<T> dr);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="field">字段实例</param>
        /// <returns></returns>
        void Query(T field, DataReader<T> dr);

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        T[] Query();
        /// <summary>
        /// 查询实段实例
        /// </summary>
        /// <param name="field">字段实例</param>
        /// <returns></returns>
        T[] Query(T field);
        /// <summary>
        /// 查询实体集分页
        /// </summary>
        /// <param name="start">开始记录</param>
        /// <param name="limit">记录数</param>
        void Query(int start, int limit, DataReader<T> dr);
        /// <summary>
        /// 查询实体集分页
        /// </summary>
        /// <param name="start">开始记录</param>
        /// <param name="limit">记录数</param>
        T[] Query(int start, int limit);
        /// <summary>
        /// 查询实体集分页
        /// </summary>
        /// <param name="field">字段实例</param>
        /// <param name="start">开始记录</param>
        /// <param name="limit">记录数</param>
        /// <returns></returns>
        T[] Query(T field, int start, int limit);
        /// <summary>
        /// 查询实体集分页
        /// </summary>
        /// <param name="field">字段实例</param>
        /// <param name="start">开始记录</param>
        /// <param name="limit">记录数</param>
        /// <returns></returns>
        void Query(T field, int start, int limit, DataReader<T> dr);
        //void QueryDistinct(T field, int start, int limit, DataReader<T> dr);
        /// <summary>
        /// 查询头一个实体
        /// </summary>
        /// <returns></returns>
        T Single();        /// <summary>
                           /// 查询头一个实体
                           /// </summary>
                           /// <param name="field">字段实例</param>
                           /// <returns></returns>
        T Single(T field);
        /// <summary>
        /// 查询一个字段，如果是“*”则返回一行记录的字典对
        /// </summary>
        /// <returns></returns>
        object Single(string field);
        /// <summary>
        /// 查询一个字段，返回一行记录的字典对
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        System.Collections.IDictionary Single(params string[] field);
        /// <summary>
        /// 求记录的个数
        /// </summary>
        /// <returns></returns>
        int Count();
        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        object Sum(string field);
        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        T Sum(T field);
        /// <summary>
        /// 求平均
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        object Avg(string field);
        /// <summary>
        /// 求平均
        /// </summary>
        T Avg(T field);
        /// <summary>
        /// 求最大值
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        object Max(string field);
        /// <summary>
        /// 求最大值
        /// </summary>
        T Max(T field);
        /// <summary>
        /// 求最小值
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns></returns>
        object Min(string field);
        /// <summary>
        /// 求最小值
        /// </summary>
        T Min(T field);
        /// <summary>
        /// 查询条件
        /// </summary>
        IWhere<T> Where
        {
            get;
        }
        /// <summary>
        /// 如果where返回值，则运行@true
        /// </summary>
        /// <param name="where"></param>
        /// <param name="true"></param>
        /// <returns></returns>
        IObjectEntity<T> IFF(System.Predicate<IObjectEntity<T>> where, System.Action<IObjectEntity<T>> @true);
        /// <summary>
        /// 如果where返回值，则运行@true，否则的运行@false
        /// </summary>
        IObjectEntity<T> IFF(System.Predicate<IObjectEntity<T>> where, System.Action<IObjectEntity<T>> @true, System.Action<IObjectEntity<T>> @false);
    }
}
