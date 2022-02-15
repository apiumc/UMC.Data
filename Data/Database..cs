using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using UMC.Data.Sql;
using UMC.Security;
namespace UMC.Data
{
    /// <summary>
    ///实体数据库访问提供器
    /// </summary>
    public sealed class Database : IDisposable
    {
        [ThreadStaticAttribute]
        static System.Collections.Hashtable _items;
        bool IsTran = false;

        /// <summary>
        /// 数据库差异配置器
        /// </summary>
        public DbProvider DbProvider
        {
            get
            {
                return DbCommonFactory.Provider;
            }
        }

        DbFactory DbCommonFactory;
        System.Data.Common.DbTransaction tran;
        /// <summary>
        /// 如果是采用事务初始化，则提交事务
        /// </summary>
        public void Commit()
        {
            if (IsTran)
            {
                if (this.DbCommonFactory.TranCmd.Transaction != null)
                {
                    this.tran.Commit();
                }
                this.DbCommonFactory.TranCmd.Connection.Close();
                this.tran.Dispose();
                this.IsTran = false;
                if (this.DbCommonFactory.Provider.Provider != null)
                {
                    if (_items != null)
                    {
                        _items.Remove(this.DbCommonFactory.Provider.Provider.Name);
                    }
                }
            }
        }
        /// <summary>
        /// 提交事务，如果threedAll为真，则提交线程中所有的事务
        /// </summary>
        /// <param name="threedAll"></param>
        public void Commit(bool threedAll)
        {
            if (threedAll)
            {
                if (_items != null)
                {
                    var d = new Database[_items.Count];
                    _items.Values.CopyTo(d, 0);
                    var m = d.GetEnumerator();
                    while (m.MoveNext())
                    {
                        var db = m.Current as Database;
                        db.Commit();
                    }
                }

            }
            else
            {
                this.Commit();
            }
        }

        /// <summary>
        /// 打开数据库接连
        /// </summary>
        public void Open()
        {
            this.DbCommonFactory.Open();
        }
        /// <summary>
        /// 关闭数据库接连
        /// </summary>
        public void Close()
        {
            this.DbCommonFactory.Close();
        }
        /// <summary>
        /// 回退事务，如果threedAll为真，则回退线程中所有的事务
        /// </summary>
        /// <param name="threedAll"></param> 
        public void Rollback(bool threedAll)
        {
            if (threedAll)
            {
                if (_items != null)
                {
                    var d = new Database[_items.Count];
                    _items.Values.CopyTo(d, 0);
                    var m = d.GetEnumerator();
                    while (m.MoveNext())
                    {
                        var db = m.Current as Database;
                        db.Rollback();
                    }
                }

            }
            else
            {
                this.Rollback();
            }
        }
        /// <summary>
        /// 如果是采用事务初始化，则回退事务
        /// </summary>
        public void Rollback()
        {
            if (IsTran)
            {
                if (this.DbCommonFactory.TranCmd.Transaction != null)
                {
                    this.tran.Rollback();
                }
                this.DbCommonFactory.TranCmd.Connection.Close();
                this.tran.Dispose();

                this.IsTran = false;
                if (_items != null)
                {
                    _items.Remove(this.DbCommonFactory.Provider.Provider.Name);
                }
            }
        }
        private Database(DbFactory DbFactory)
        {
            this.DbCommonFactory = DbFactory;
        }
        /// <summary> 
        /// 创建默认DbProvider实体访实例,默认配置节点是defaultDbProvider
        /// </summary>
        /// <returns></returns>
        public static Database Instance()
        {
            return Instance("defaultDbProvider"); ;
        }
        /// <summary>
        /// 创建DataBase
        /// </summary>
        /// <param name="Factor"></param>
        /// <returns></returns>
        public static Database Instance(DbFactory Factor)
        {
            return new Database(Factor);
        }
        public Database For(Guid appKey)
        {
            if (this.DbCommonFactory.Provider != null && this.DbCommonFactory.Provider.Provider != null)
            {
                var provider = this.DbCommonFactory.Provider.Provider;
                var provider2 = Reflection.CreateObject(Reflection.Instance().DatabaseProvider(this.DbCommonFactory.Provider.Provider.Name, appKey)) as DbProvider;
                return Instance(new DbFactory(provider2));
            }
            return this;
        }
        /// <summary>
        /// 创建默认数据库实体访实例
        /// </summary>
        /// <param name="providerName">配置节点名</param>
        /// <returns></returns>
        public static Database Instance(string providerName)
        {
            if (_items != null)
            {
                if (_items.ContainsKey(providerName))
                {
                    return _items[providerName] as Database;
                }
            }
            var pKey = UMC.Security.Principal.Current.AppKey ?? Guid.Empty;

            var node = Reflection.Instance().DatabaseProvider(providerName, pKey);
            if (node == null)
            {
                //if (pKey == Guid.Empty && String.Equals(providerName, "defaultDbProvider"))
                //{
                //    UMC.Data.Sql.Initializer.IsSetup = true;
                //}
                throw new Data.Sql.DbException(new Exception(String.Format("未配置“{0}”", providerName)), null);

            }
            var provider = Reflection.CreateObject(node) as DbProvider;
            return new Database(new DbFactory(provider));
        }


        /// <summary>
        /// 使用事务，如果已经使用了事务，则返回false，如果没有使用事务，则取用事务并返回true
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction()
        {
            return this.BeginTransaction(System.Data.IsolationLevel.Unspecified);
        }
        /// <summary>
        /// 使用事务，如果已经使用了事务，则返回false，如果没有使用事务，则取用事务并返回true
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction(System.Data.IsolationLevel lev)
        {
            if (IsTran == false)
            {
                this.tran = this.DbCommonFactory.UseTran(lev);
                this.IsTran = true;

                if (this.DbCommonFactory.Provider.Provider != null)
                {
                    if (String.IsNullOrEmpty(this.DbCommonFactory.Provider.Provider.Name) == false)
                    {
                        if (_items == null)
                        {
                            _items = new System.Collections.Hashtable();
                        }
                        _items[this.DbCommonFactory.Provider.Provider.Name] = this;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 创建的Sql语句的查询器
        /// </summary>
        /// <returns></returns>
        public ISqler Sqler()
        {
            return DbCommonFactory.Sqler();
        }
        /// <summary>
        /// 创建的Sql语句的查询器
        /// </summary>
        /// <param name="TimeOut">超时时间</param>
        /// <returns></returns>
        public ISqler Sqler(int TimeOut)
        {
            return DbCommonFactory.Sqler(TimeOut);
        }
        /// <summary>
        /// 创建的Sql语句的查询器
        /// </summary>
        /// <param name="pfx">是否自动添加表前缀</param>
        /// <returns></returns>
        public ISqler Sqler(bool pfx)
        {
            return DbCommonFactory.Sqler(pfx);
        }

        /// <summary>
        /// 创建实体综合管理适配器
        /// </summary>
        /// <returns></returns>
        public IObjectEntity<T> ObjectEntity<T>() where T : class
        {
            return DbCommonFactory.ObjectEntity<T>();// (TimeOut);
        }

        /// <summary>
        /// 创建实体综合管理适配器
        /// </summary>
        /// <returns></returns>
        public IObjectEntity<T> ObjectEntity<T>(string tabName, T anonymousType) where T : class
        {
            return DbCommonFactory.ObjectEntity<T>(tabName);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            ((IDisposable)DbCommonFactory).Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}







