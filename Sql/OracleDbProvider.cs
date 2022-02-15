using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace UMC.Data.Sql
{
    class OracleBuilder : DbBuilder
    {
        public override string AddColumn(string name, string field, string type)
        {
            return string.Format("ALTER TABLE {0} ADD({1} {2})", name, field, type);
        }
        public override string Binary()
        {
            return "BLOB";
        }
        public override string Boolean()
        {
            return "NUMBER";
        }

        public override string Date()
        {
            return "DATE";
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
            return "NUMBER";
        }
        public override string Column(string field)
        {
            return field.ToUpper();
        }

        public override string Number()
        {
            return "NUMBER(16,2)";
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
            return "NVARCHAR2(500)";
        }

        public override string Text()
        {
            return "VARCHAR2(4000)";
        }
        public override bool? Check(string name, string field, ISqler sqler)
        {

            return Convert.ToInt32(sqler.ExecuteScalar("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = {0} AND COLUMN_NAME = {1}", name.ToUpper(), field)) > 0;
        }
        public override bool? Check(string name, ISqler sqler)
        {
            return Convert.ToInt32(sqler.ExecuteScalar("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME = {0}", name.ToUpper())) > 0;
        }
    }

    public class OracleDbProvider : UMC.Data.Sql.DbProvider
    {
        public override DbBuilder Builder => new OracleBuilder();
        public static System.Data.Common.DbProviderFactory Instance;
        public override System.Data.Common.DbProviderFactory DbFactory
        {
            get
            {

                if (Instance == null)
                {
                    var als = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var a in als)//mscorlib, 
                    {
                        var type = a.GetType("Oracle.ManagedDataAccess.Client.OracleClientFactory");
                        if (type != null)
                        {
                            Instance = type.GetField("Instance").GetValue(null) as System.Data.Common.DbProviderFactory;

                            break;
                        }
                    }
                    if (Instance == null)
                    {
                        throw new Exception("请引用Oracle.ManagedDataAccess.dll");

                    }
                }
                return Instance;
            }
        }

        public override string ConntionString
        {
            get
            {
                return this.Provider.Attributes["conString"];
            }
        }
        public override string QuotePrefix => "";
        public override string QuoteSuffix => "";


        protected override string ParamsPrefix => ":";

        public override string GetIdentityText(string tableName)
        {
            return String.Empty;
        }
        public override string AppendDbParameter(string key, object obj, DbCommand cmd)
        {
            if (obj is Guid)
            {
                obj = obj.ToString();
            }
            else if (obj is Boolean)
            {
                return base.AppendDbParameter(key, ((Boolean)obj) ? 1 : 0, cmd);
            }
            else if (obj is Enum)
            {
                return base.AppendDbParameter(key, Convert.ToInt32(obj), cmd);
            }
            return base.AppendDbParameter(key, obj, cmd);
        }

        public override string GetPaginationText(int start, int limit, string selectText)
        {
            return String.Format(@"SELECT * FROM (SELECT A.*, ROWNUM R__ FROM ({2}) A WHERE ROWNUM <= {1})WHERE R__ > {0} ", start, start + limit, selectText);
        }
    }
}