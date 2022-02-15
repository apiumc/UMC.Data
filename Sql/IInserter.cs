using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 实体插入适配器
    /// </summary>
    public interface IInserter
    {
        /// <summary>
        /// 批量插入实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ages"></param>
        /// <returns></returns>
        bool Execute<T>(params T[] ages);

        /// <summary>
        /// 插入数据，采用表名和字典更新数据
        /// </summary>
        /// <param name="name">表名</param>
        /// <param name="values">更新的字段对</param>
        /// <returns>返回受影响的行数</returns>
        int Execute(string name, params System.Collections.IDictionary[] values);

        /// <summary>
        /// 插入单一实体对象，返回当前表的@@identity
        /// </summary>
        /// <typeparam name="T">简单对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        int ExecuteSingle(object obj);
        /// <summary>
        /// 把数据表的行插入数据库表中
        /// </summary>
        /// <param name="table">表</param>
        /// <returns></returns>
        bool Execute(System.Data.DataTable table);
        /// <summary>
        /// 把数据表的行插入数据库表中
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        int Execute(System.Data.IDataReader reader, string table);
    }
}
