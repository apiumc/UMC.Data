using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data
{
    /// <summary>
    /// 多层数据库接口
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// 用前缀创建数据访问层
        /// </summary>
        /// <param name="prefix">数据库前缀</param>
        /// <returns></returns>
        UMC.Data.Database DataBase(string prefix);
        /// <summary>
        /// 创建带前缀的表
        /// </summary>
        /// <typeparam name="T">实体表类型</typeparam>
        /// <param name="prefix">数据库前缀</param>
        /// <returns></returns>
        bool Create<T>(string prefix) where T : class;
        /// <summary>
        /// 创建带前缀的表
        /// </summary>
        /// <param name="prefix">数据库前缀</param>
        /// <param name="name">数据库前缀</param>
        /// <returns></returns>
        bool CreateTable(string prefix, string name);

        /// <summary>
        /// 判断是否存在当前这个表
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Exists(string prefix, string name);
        /// <summary>
        /// 判断是否存在当前实体表
        /// </summary>
        /// <typeparam name="T">实体表类型</typeparam>
        /// <param name="prefix"></param>
        /// <returns></returns>
        bool Exists<T>(string prefix) where T : class;
    }
}
