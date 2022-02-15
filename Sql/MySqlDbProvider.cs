using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace UMC.Data.Sql
{
    class MySqlBuilder : DbBuilder
    {
        public override string AddColumn(string name, string field, string type)
        {
            return string.Format("ALTER TABLE {0} ADD {1} {2} ", name, field, type);
        }
        public override string Binary()
        {
            return "blob";
        }
        public override string Boolean()
        {
            return "TINYINT";
        }

        public override string Date()
        {
            return "DATETIME";
        }

        public override string DropColumn(string name, string field)
        {
            return string.Format("ALTER TABLE {0} DROP {1}", name, field);
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
                sb.AppendFormat("{0}", s);
                sb.Append(',');

            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");
            return sb.ToString();
        }

        public override string String()
        {
            return "NVARCHAR(500)";
        }

        public override string Text()
        {
            return "TEXT";
        }
        public override bool? Check(string name, string field, ISqler sqler)
        {

            int m = Convert.ToInt32(sqler.ExecuteScalar("select count(*) from information_schema.columns where table_name = {0} and column_name = {1}", name, field));
            return m > 0;
        }
        public override bool? Check(string name, ISqler sqler)
        {
            int m = Convert.ToInt32(sqler.ExecuteScalar("select count(*) from information_schema.tables where table_name = {0}", name));
            return m > 0;
        }

    }

    public class MySqlDbProvider : UMC.Data.Sql.DbProvider
    {
        public static System.Data.Common.DbProviderFactory Instance;
        public override string AppendDbParameter(string key, object obj, DbCommand cmd)
        {
            if (obj is Guid)
            {
                obj = obj.ToString();
            }
            else if (obj is Enum)
            {
                return base.AppendDbParameter(key, Convert.ToInt32(obj), cmd);
            }
            return base.AppendDbParameter(key, obj, cmd);
        }
        public override string ConntionString
        {
            get
            {
                return this.Provider.Attributes["conString"];
            }
        }
        public override DbBuilder Builder => new MySqlBuilder();


        public override string GetIdentityText(string tableName)
        {
            return String.Empty;
        }
        protected override string ParamsPrefix => "?";
        public override string QuotePrefix
        {
            get
            {
                return "`";
            }
        }
        public override string QuoteSuffix
        {
            get
            {
                return "`";
            }
        }
        public override string GetPaginationText(int start, int limit, string selectText)
        {
            return String.Format("{0} limit {1} offset {2}", selectText, limit, start);
        }
        public override System.Data.Common.DbProviderFactory DbFactory
        {
            get
            {

                if (Instance == null)
                {
                    var als = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var a in als)//mscorlib, 
                    {
                        var type = a.GetType("MySql.Data.MySqlClient.MySqlClientFactory");
                        if (type != null)
                        {
                            Instance = type.GetField("Instance").GetValue(null) as System.Data.Common.DbProviderFactory;

                            break;
                        }
                    }
                    if (Instance == null)
                    {
                        throw new Exception("请引用MySql.Data.dll");

                    }
                }
                return Instance;
            }
        }

    }
}
