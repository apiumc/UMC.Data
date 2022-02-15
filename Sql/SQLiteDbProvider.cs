using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace UMC.Data.Sql
{


    class SQLiteBuilder : DbBuilder
    {
        public override string AddColumn(string name, string field, string type)
        {
            return string.Format("ALTER TABLE {0} ADD COLUMN {1} {2} ", name, field, type);
        }

        public override string Binary()
        {
            return "BLOB";
        }

        public override string Boolean()
        {
            return "INTEGER";
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
            return "REAL";
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
            return "NUMERIC";
        }

        public override string PrimaryKey(string name, params string[] fields)
        {
            return string.Empty;
        }

        public override string String()
        {
            return "CHAR(255)";
        }

        public override string Text()
        {
            return "CHAR";
        }
        public override bool? Check(string name, ISqler sqler)
        {

            int m = Convert.ToInt32(sqler.ExecuteScalar("select count(*)  from sqlite_master where type ={1} and name = {0}", name, "table"));
            return m > 0;
        }
        public override bool? Check(string name, string field, ISqler sqler)
        {
            var bc = false;
            sqler.Execute(System.String.Format("PRAGMA  table_info([{0}])", name), dr =>
              {
                  while (dr.Read())
                  {
                      bc = System.String.Equals(dr["name"] as string, field, StringComparison.CurrentCultureIgnoreCase);
                      if (bc)
                      {
                          return;
                      }

                  }
              });
            return bc;
        }
    }

    public class SQLiteDbProvider : UMC.Data.Sql.DbProvider
    {
        public static System.Data.Common.DbProviderFactory Instance;
        public override string AppendDbParameter(string key, object obj, DbCommand cmd)
        {
            if (obj is Guid)
            {
                obj = obj.ToString();
            }
            return base.AppendDbParameter(key, obj, cmd);
        }
        public override string ConntionString
        {
            get
            {
                var path = UMC.Data.Reflection.AppDataPath(this.Provider["db"] ?? "umc.db");
                return String.Format("Data Source={0};Cache=Shared", path);
            }
        }
        public override DbBuilder Builder => new SQLiteBuilder();

        public override string QuotePrefix
        {
            get
            {
                return "[";
            }
        }
        public override string QuoteSuffix
        {
            get
            {
                return "]";
            }
        }

        public override string GetIdentityText(string tableName)
        {
            return String.Empty;
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
                        var type = a.GetType("System.Data.SQLite.SQLiteFactory");
                        if (type != null)
                        {
                            Instance = type.GetField("Instance").GetValue(null) as System.Data.Common.DbProviderFactory;

                            break;
                        }
                    }
                    if (Instance == null)
                    {
                        throw new Exception("请引用SQLite ADO.NET");

                    }
                }
                return Instance;
            }
        }

    }
}
