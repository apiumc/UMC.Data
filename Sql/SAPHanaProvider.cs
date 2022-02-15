using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace UMC.Data.Sql
{
    class SAPHanaBuilder : DbBuilder
    {
        public override string AddColumn(string name, string field, string type)
        {
            return string.Format("ALTER TABLE {0} ADD({1} {2})", name, field, type);
        }
        public override string Binary()
        {
            return "VARBINARY(5000)";
        }
        public override string Boolean()
        {
            return "BOOLEAN";
        }

        public override string Date()
        {
            return "DATETIME";
        }

        public override string DropColumn(string name, string field)
        {
            return string.Format("ALTER TABLE {0} DROP COLUMN {1}", name, field);
        }

        public override string Float()
        {
            return "FLOAT";
        }

        public override string Guid()
        {
            return "CHAR(36)";
        }

        public override string Integer()
        {
            return "INTEGER";
        }
        public override string Column(string field)
        {
            return field.ToUpper();
        }

        public override string Number()
        {
            return "DECIMAL(16,2)";
        }

        public override string PrimaryKey(string name, params string[] fields)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("ALTER TABLE {0} ADD PRIMARY KEY (", name);// id);")
            foreach (var s in fields)
            {
                sb.AppendFormat("{0}", s.ToUpper());
                sb.Append(',');

            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            return sb.ToString();
        }

        public override string String()
        {
            return "NVARCHAR(255)";
        }

        public override string Text()
        {
            return "NVARCHAR(4000)";
        }
        public override bool? Check(string name, string field, ISqler sqler)
        {

            return Convert.ToInt32(sqler.ExecuteScalar("SELECT COUNT(*) FROM SYS.TABLE_COLUMNS  WHERE TABLE_NAME = {0} AND COLUMN_NAME = {1}", name.ToUpper(), field.ToUpper())) > 0;
        }
        public override bool? Check(string name, ISqler sqler)
        {
            return Convert.ToInt32(sqler.ExecuteScalar("SELECT COUNT(*) FROM SYS.M_TABLES WHERE TABLE_NAME = {0}", name.ToUpper())) > 0;
        }
    }
    public class SAPHanaProvider : UMC.Data.Sql.DbProvider
    {
        public override DbBuilder Builder => new SAPHanaBuilder();
        public static System.Data.Common.DbProviderFactory Instance;
        public override string AppendDbParameter(string key, object obj, DbCommand cmd)
        {
            if (obj is String)
            {
                return String.Format("'{0}'", ((String)(obj)).Replace("'", "\'"));
            }
            else if (obj is Boolean)
            {
                return ((Boolean)obj) ? "1" : "0";
            }
            else if (obj is Enum)
            {
                return Convert.ToInt32(obj).ToString();


            }
            else if (obj is byte[])
            {
                return "'" + BitConverter.ToString((byte[])obj) + "'";
            }
            else
            {
                return obj.ToString();
            }
        }
        public override string ConntionString
        {
            get
            {
                return this.Provider.Attributes["conString"];
            }
        }
        public override System.Data.Common.DbProviderFactory DbFactory
        {
            get
            {
                if (Instance == null)
                {
                    throw new Exception("请引用HanaFactory");
                }
                return Instance;
            }
        }

        public override string GetIdentityText(string tableName)
        {

            return String.Empty;
        }
        protected override string ParamsPrefix => "";
        public override string QuotePrefix => "\"";
        public override string QuoteSuffix => "\"";
        public override string GetPaginationText(int start, int limit, string selectText)
        {
            return String.Format("{0} limit {1} offset {2}", selectText, limit, start);
        }
    }

}