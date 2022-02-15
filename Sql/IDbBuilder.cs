using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    public abstract class DbBuilder
    {
        /// <summary>
        /// 增加主键的SQL
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public abstract string PrimaryKey(string name, params string[] fields);
        /// <summary>
        /// 创建表空间
        /// </summary>
        /// <param name="Prefixion"></param>
        /// <returns></returns>
        public virtual string Schema(string Prefixion)
        {
            return string.Empty;
        }
        public virtual string Column(string field)
        {
            return field;// String.Empty;
        }
        public abstract string Binary();
        public abstract string Integer();
        public abstract string Boolean();
        public abstract string String();
        public abstract string Text();

        public abstract string Number();
        public abstract string Date();
        public abstract string Guid();
        public abstract string Float();

        /// <summary>
        /// 增加列的SQL
        /// </summary>
        public abstract string AddColumn(string name, string field, string type);
        /// <summary>
        /// 删除列的SQL
        /// </summary>
        public abstract string DropColumn(string name, string field);

        /// <summary>
        /// 检测列名
        /// </summary>
        public virtual bool? Check(string name, string field, ISqler sqler)
        {
            return null;
        }
        /// <summary>
        /// 检测表名
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sqler"></param>
        /// <returns></returns>
        public virtual bool? Check(string name, ISqler sqler)
        {
            return null;
        }
    }
}
