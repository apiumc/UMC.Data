using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data
{
    /// <summary>
    /// 自定义JSON序列与反序列化接口
    /// </summary>
    public interface IJSON
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer"></param>
        void Write(System.IO.TextWriter writer);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="key">属性名</param>
        /// <param name="value">属性值</param>
        void Read(string key, object value);
    }
    /// <returns></returns>
    /// <summary>
    /// 处理方法接口
    /// </summary>
    public interface IDoWork
    {
        /// <summary>
        /// 执行的方法
        /// </summary>
        /// <param name="dictionary">字典参数</param>
        void Do(System.Collections.IDictionary dictionary);
    }
}
