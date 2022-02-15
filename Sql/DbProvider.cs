using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace UMC.Data.Sql
{
    /// <summary>
    /// 数据差异管理提供器
    /// </summary>
    public abstract class DbProvider : UMC.Data.DataProvider
    {

        /// <summary>
        /// 数据联接字符串
        /// </summary>
        public abstract string ConntionString
        {
            get;
        }
        /// <summary>
        /// 追加参数，同时返回数据库参数名
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">参数值</param>
        /// <param name="cmd">命令</param>
        /// <returns></returns>
        public virtual string AppendDbParameter(string key, object obj, DbCommand cmd)
        {
            if (String.IsNullOrEmpty(ParamsPrefix) == false)
            {
                if (key.StartsWith(ParamsPrefix) == false)
                {
                    key = ParamsPrefix + key;
                }
                if (cmd.Parameters.Contains(key))
                {
                    return key;
                }
            }
            var parm = this.DbFactory.CreateParameter();
            parm.ParameterName = key;
            if (obj == null)
            {
                parm.Value = DBNull.Value;
            }
            else
            {
                if (obj is DateTime)
                {
                    parm.DbType = System.Data.DbType.DateTime;
                    if (Convert.ToDateTime(obj) == DateTime.MinValue)
                    {
                        parm.Value = DBNull.Value;
                    }
                    else
                    {
                        parm.Value = obj;
                    }

                }
                else
                {

                    parm.Value = obj;
                }
            }
            cmd.Parameters.Add(parm);
            if (string.IsNullOrEmpty(this.ParamsPrefix))
            {
                return "?";
            }
            return key;

        }

        /// <summary>
        /// 参数前缀
        /// </summary>
        protected virtual string ParamsPrefix
        {
            get
            {
                return "@";
            }
        }
        public virtual DbBuilder Builder
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// 数据访问
        /// </summary>
        /// <returns></returns>
        public abstract System.Data.Common.DbProviderFactory DbFactory
        {
            get;
        }
        /// <summary>
        /// 实体映射表名前缀
        /// </summary>
        public virtual string Prefixion
        {
            get
            {
                if (this.Provider != null)
                {
                    return this.Provider.Attributes["prefix"] ?? "";
                }
                return "";

            }
        }

        /// <summary>
        /// 实体映射表名前缀分割符
        /// </summary>
        public virtual string Delimiter
        {
            get
            {
                if (this.Provider != null)
                {

                    return this.Provider.Attributes["delimiter"] ?? ".";

                }
                return ".";

            }
        }
        /// <summary>
        ///  指定其名称包含空格或保留标记等字符的数据库对象（例如，表或列）时使用的起始字符。
        /// </summary>
        public virtual string QuotePrefix
        {
            get
            {
                return "";
            }
        }
        /// <summary>
        /// 指定其名称包含空格或保留标记等字符的数据库对象（例如，表或列）时使用的结束字符
        /// </summary>
        public virtual string QuoteSuffix
        {
            get
            {
                return "";
            }
        }
        /// <summary>
        /// 取得自增列的Sql脚本
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public abstract string GetIdentityText(string tableName);
        /// <summary>
        /// 合成分页的Sql格式脚本，注意本分页是采用Top进去分页，如果你的数据源不支持Top运算，请重写
        /// </summary>
        /// <param name="start">开始记录数)</param>
        /// <param name="limit">后面的条数</param>
        /// <param name="selectText">查询的Sql脚本</param>
        /// <returns></returns>
        public abstract string GetPaginationText(int start, int limit, string selectText);
    }
}
