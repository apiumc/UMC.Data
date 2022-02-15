using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    /// <summary>
    /// SQL格式化错误
    /// </summary>
    public class SqlFormatException : FormatException
    {
        string name;
        /// <summary>
        /// SQL格式化错误
        /// </summary>
        public SqlFormatException(string name)
        {
            this.name = name;
        }
        public override string Message
        {
            get
            {
                return "在使用自动前缀时，表对象" + this.name + "带“.”";
            }
        }
    }
    /// <summary>
    /// 数据处理工厂处理异常
    /// </summary>
    public class DbException : Exception
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ex">原始异常</param>
        /// <param name="cmd">合成数据库访问命令</param>
        public DbException(Exception ex, System.Data.Common.DbCommand cmd)
            : base(ex.Message, ex)
        {
            this.cmd = cmd;
        }

        System.Data.Common.DbCommand cmd;
        /// <summary>
        /// 数据处理工厂合成的的Sql命令
        /// </summary>
        public System.Data.Common.DbCommand DbCommand
        {
            get
            {
                return cmd;
            }
        }
    }

}
