using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 运行CommandRun的辅助代理
    /// </summary>
    /// <param name="cmdRun">运行Command的代理</param>
    /// <returns></returns>
    delegate object CommandProgress(CommandRun cmdRun);
    /// <summary>
    /// 运行Command的代理
    /// </summary>
    /// <param name="cmd">表示要对数据源执行的 SQL 语句或存储过程。为表示命令的、数据库特有的类提供一个基类</param>
    /// <returns></returns>
    delegate object CommandRun(DbCommand cmd);

    /// <summary>
    /// 处理数据代理
    /// </summary>
    /// <param name="dr"></param>
    public delegate void DataRecord(System.Data.IDataRecord dr);
    /// <summary>
    /// 处理数据代理
    /// </summary>
    /// <param name="dr"></param>
    public delegate void DataReader(System.Data.IDataReader dr);
    /// <summary>
    /// 处理数据代理
    /// </summary>
    /// <typeparam name="T">实体类</typeparam>
    /// <param name="item"></param>
    public delegate void DataReader<T>(T item);
    /// <summary>
    /// 实体架构数据层访问层工厂，支持生命周期事务管理
    /// </summary>
    public class DbFactory : IDisposable
    {

        /// <summary>
        /// 数据库差异配置提供者
        /// </summary>
        public DbProvider Provider
        {
            get;
            private set;
        }
        int TimeOut = 30000;
        /// <summary>
        /// 用数据连接字符串初始化访问工厂
        /// </summary>
        /// <param name="dbProvider">访问管理器</param>
        public DbFactory(DbProvider dbProvider)
        {
            this.Provider = dbProvider;
            this.Progress = this.ConnectionProgress;
        }
        CommandProgress Progress;
        internal DbCommand TranCmd
        {
            get;
            set;
        }

        object TrasactionProgress(CommandRun cmdAcs)
        {
            this.TranCmd.CommandTimeout = TimeOut;
            this.TranCmd.Parameters.Clear();
            this.TranCmd.CommandText = String.Empty;
            switch (this.TranCmd.Connection.State)
            {
                case ConnectionState.Closed:
                    return ConnectionProgress(cmdAcs);
                default:

                    try
                    {
                        return cmdAcs(this.TranCmd);
                    }
                    catch (System.Data.Common.DbException ex)
                    {
                        throw new DbException(ex, this.TranCmd);
                    }
            }


        }
        object ConnectionProgress(CommandRun cmdAcs)
        {

            using (DbConnection con = Provider.DbFactory.CreateConnection())
            {
                con.ConnectionString = this.Provider.ConntionString;
                DbCommand cmd = con.CreateCommand();
                cmd.CommandTimeout = TimeOut;
                cmd.Connection.Open();
                try
                {
                    return cmdAcs(cmd);
                }
                catch (System.Data.Common.DbException ex)
                {
                    throw new DbException(ex, cmd);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        DbConnection conOpen;
        /// <summary>
        /// 打开数据连接
        /// </summary>
        public void Open()
        {
            if (conOpen == null)
            {
                conOpen = Provider.DbFactory.CreateConnection();
                conOpen.ConnectionString = Provider.ConntionString;
            }
            conOpen.Open();
            this.Progress = CommandProgress;
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {

            if (conOpen != null)
            {
                conOpen.Close();
            }
        }
        object CommandProgress(CommandRun cmdAcs)
        {

            DbCommand cmd = conOpen.CreateCommand();
            cmd.CommandTimeout = TimeOut;
            try
            {
                return cmdAcs(cmd);
            }
            catch (System.Exception ex)
            {
                throw new DbException(ex, cmd);
            }
        }

        /// <summary>
        /// 创建实体综合管理适配器
        /// </summary>
        /// <returns></returns>
        public IObjectEntity<T> ObjectEntity<T>() where T : class
        {
            ObjectEntity<T> selecter = new ObjectEntity<T>(new Sqler(Provider, Progress, true), String.Empty);
            return selecter;
        }
        /// <summary>
        /// 创建实体综合管理适配器
        /// </summary>
        /// <returns></returns>
        public IObjectEntity<T> ObjectEntity<T>(string tabName) where T : class
        {
            ObjectEntity<T> selecter = new ObjectEntity<T>(new Sqler(Provider, Progress), tabName);
            return selecter;
        }
        /// <summary>
        /// 使用事务
        /// </summary>
        public DbTransaction UseTran(IsolationLevel isolationLevel)
        {
            DbConnection con = Provider.DbFactory.CreateConnection();//
            con.ConnectionString = this.Provider.ConntionString;
            con.Open();
            DbTransaction tran = con.BeginTransaction(isolationLevel);
            this.Progress = this.TrasactionProgress;
            this.TranCmd = tran.Connection.CreateCommand();
            this.TranCmd.Transaction = tran;
            return tran;
        }

        /// <summary>
        /// 创建sql适配器
        /// </summary>
        /// <returns></returns>
        public ISqler Sqler()
        {
            Sqler sqler = new Sqler(Provider, Progress);
            return sqler;
        }
        /// <summary>
        /// 创建sql适配器
        /// </summary>
        /// <returns></returns>
        public ISqler Sqler(bool pfx)
        {
            Sqler sqler = new Sqler(Provider, Progress, pfx);
            return sqler;
        }
        /// <summary>
        /// 创建sql适配器
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public ISqler Sqler(int timeout)
        {
            return this.Sqler(timeout, true);
        }
        /// <summary>
        /// 创建sql适配器
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <param name="pfx">前缀</param>
        /// <returns></returns>
        public ISqler Sqler(int timeout, bool pfx)
        {
            this.TimeOut = timeout;
            Sqler sqler = new Sqler(Provider, Progress, pfx);
            return sqler;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (TranCmd != null)
            {
                TranCmd.Dispose();
            }
            if (conOpen != null)
            {
                conOpen.Dispose();
            }
        }

        #endregion
    }
}
