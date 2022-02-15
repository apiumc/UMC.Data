using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// sql分析器
    /// </summary>
    public interface ISqler : IScript
    {
        /// <summary>
        ///  执行 SQL 语句。
        /// </summary>
        /// <param name="sqlText">索引格式化脚本</param>
        /// <param name="paramers">参数</param>
        /// <returns>返回影响行数</returns>
        int ExecuteNonQuery(string sqlText, params object[] paramers);
        /// <summary>
        ///  执行 SQL 语句。
        /// </summary>
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        /// <returns>返回影响行数</returns>
        int ExecuteNonQuery(string sqlText, System.Collections.IDictionary paramKeys);
        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="sqlText">索引格式化脚本</param>
        /// <param name="paramers">参数</param>
        /// <returns>结果集中第一行的第一列。</returns>
        object ExecuteScalar(string sqlText, params object[] paramers);

        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        object ExecuteScalar(string sqlText, System.Collections.IDictionary paramKeys);
        /// <summary>
        ///  在 System.Data.DataSet 中添加或刷新行以匹配使用 System.Data.DataSet 名称的数据源中的行，并创建一个 System.Data.DataTable。
        /// </summary>
        /// <param name="sqlText">索引格式化脚本</param>
        /// <param name="paramers">参数</param>
        /// <returns>包含从数据源返回的架构信息的 System.Data.DataTable 对象。</returns>
        System.Data.DataTable ExecuteTable(string sqlText, params object[] paramers);
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        System.Data.DataTable ExecuteTable(string sqlText, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 返回分页的数据表
        /// </summary>
        /// <param name="sqlText">sql查询文本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        System.Data.DataTable Execute(string sqlText, int start, int limit, params object[] paramers);
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="paramKeys">字典参数</param>
        System.Data.DataTable Execute(string sqlText, int start, int limit, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 返回DataSet,注意查询不能包含20个表集
        /// </summary>
        /// <param name="sqlText">索引格式化脚本</param>
        /// <param name="paramers">参数</param>
        /// <returns>返回一个DataSet</returns>
        System.Data.DataSet ExecuteDataSet(string sqlText, params object[] paramers);
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        System.Data.DataSet ExecuteDataSet(string sqlText, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 返回查询的第一行，并把他字典
        /// </summary>
        /// <param name="sqlText">索引格式化脚本</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        System.Collections.IDictionary ExecuteSingle(string sqlText, params object[] paramers);
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        System.Collections.IDictionary ExecuteSingle(string sqlText, System.Collections.IDictionary paramKeys);

        /// <summary>
        /// 把查询到的字段转化对应的单个实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sqlText">SQL查询文本</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        T ExecuteSingle<T>(string sqlText, params object[] paramers);
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        T ExecuteSingle<T>(string sqlText, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 把查询到的字段转化对应的实体集
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sqlText">SQL查询文本</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        List<T> Execute<T>(string sqlText, params object[] paramers);
        /// <summary>
        /// 把查询到的字段转化对应的实体集
        /// </summary>
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="paramKeys">字典参数</param>
        List<T> Execute<T>(string sqlText, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 自定义处理一个查询只读的结果集
        /// </summary>
        /// <param name="sqlText">SQL查询文本</param>
        /// <param name="reader">处理数据集代理</param>
        /// <param name="paramers">参数</param>
        void Execute<T>(string sqlText, DataReader<T> reader, params object[] paramers);
        /// <summary>
        /// 返回分页的数据表
        /// </summary>
        /// <param name="sqlText">sql查询文本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="reader">处理数据集代理</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        void Execute<T>(string sqlText, int start, int limit, DataReader<T> reader, params object[] paramers);

        /// <summary>
        /// 返回分页的数据表
        /// </summary>
        /// <param name="sqlText">sql查询文本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        List<T> Execute<T>(string sqlText, int start, int limit, params object[] paramers);
        /// <summary>
        /// 返回分页的数据表
        /// </summary>
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="paramKeys">字典参数</param>
        List<T> Execute<T>(string sqlText, int start, int limit, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 自定义处理一个查询只读的结果集
        /// </summary>
        /// <param name="sqlText">SQL查询文本</param>
        /// <param name="reader">处理数据集代理</param>
        /// <param name="paramers">参数</param>
        void Execute(string sqlText, DataReader reader, params object[] paramers);


        /// <summary>
        /// 处理分页的数据集，注意：此DataReader是已经Read过的，请在reader中不要调用Read方法
        /// </summary>
        /// <param name="sqlText">sql查询文本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="reader">处理数据集代理</param>
        /// <param name="paramers">参数</param>
        /// <returns></returns>
        void Execute(string sqlText, int start, int limit, DataRecord reader, params object[] paramers);
        /// <summary>
        /// 处理分页的数据集，注意：此DataReader是已经Read过的，请在reader中不要调用Read方法
        /// </summary>
        /// <param name="sqlText">字典格式化脚本</param>
        /// <param name="start">开始位置</param>
        /// <param name="limit">记录数</param>
        /// <param name="reader">处理数据集代理</param>
        /// <param name="paramKeys">字典参数</param>
        void Execute(string sqlText, int start, int limit, DataRecord reader, System.Collections.IDictionary paramKeys);

        /// <summary>
        /// 自定义处理一个查询只读的结果集
        /// </summary>
        /// <param name="sqlText">SQL查询文本</param>
        /// <param name="reader">处理数据集代理</param>
        /// <param name="paramKeys">参数字典</param>
        void Execute(string sqlText, DataReader reader, System.Collections.IDictionary paramKeys);
        /// <summary>
        /// 批量执行脚本，返回每个脚本的返回影响行数
        /// </summary>
        /// <param name="paramKeys"></param>
        /// <param name="sqlTexts"></param>
        /// <returns></returns>
        int[] ExecuteNonQuery(System.Collections.IDictionary paramKeys, params string[] sqlTexts);
        /// <summary>
        /// 批量执行脚本，返回每个脚本的返回影响行数
        /// </summary>
        /// <param name="scripts"></param>
        /// <returns></returns>
        int[] ExecuteNonQuery(params Script[] scripts);

        /// <summary>
        /// 批量执行脚本 ， 当predicate返回true才执行action，并运行在action中设置script对象的脚本参数
        /// </summary>
        /// <param name="predicate">当predicate返回为true时，则会回调以Script，参数初始化cmdexec</param>
        /// <param name="cmdexec">执行的脚本</param>
        void Execute(System.Predicate<Script> predicate, System.Action<System.Data.Common.DbCommand> cmdexec);
    }

}
