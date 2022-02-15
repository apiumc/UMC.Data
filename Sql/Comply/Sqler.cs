using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.Common;

namespace UMC.Data.Sql
{
    class Sqler : ISqler
    {
        public Sqler(DbProvider dbProvider, CommandProgress progress)
        {
            this.dbProvider = dbProvider;
            this.Command = progress;
            this.isAutoPfx = true;
        }

        public Sqler(DbProvider dbProvider, CommandProgress progress, bool autoPfx)
        {
            this.dbProvider = dbProvider;
            this.Command = progress;
            this.isAutoPfx = autoPfx;
        }
        bool isAutoPfx;
        DbProvider dbProvider;

        public DbProvider DbProvider
        {
            get
            {
                return this.dbProvider;
            }

        }
        CommandProgress Command;
        object Progress(CommandRun cmdAcs, string sqlText, params object[] paramers)
        {
            return Command(cmd =>
            {
                cmd.Parameters.Clear();
                switch (paramers.Length)
                {
                    case 1:
                        if (paramers[0] is System.Collections.IDictionary)
                        {

                            cmd.CommandText = SqlParamer.Format(this.dbProvider, this.isAutoPfx, cmd, sqlText, (System.Collections.IDictionary)paramers[0]);
                        }
                        else
                        {
                            goto default;
                        }

                        break;
                    default:
                        cmd.CommandText = SqlParamer.Format(this.dbProvider, this.isAutoPfx, cmd, sqlText, paramers);
                        break;
                }
                //cmd.Parameters[0]
                //UMC.Data.Utility.Log("SQL", cmd.CommandText, paramers);
                this.script = Script.Create(cmd.CommandText, paramers);

                return cmdAcs(cmd);

            });

        }

        int ISqler.ExecuteNonQuery(string sqlText, params object[] paramers)
        {
            return (int)Progress(cmd => cmd.ExecuteNonQuery(), sqlText, paramers);
        }
        object ISqler.ExecuteScalar(string sqlText, params object[] paramers)
        {
            return Progress(cmd => cmd.ExecuteScalar(), sqlText, paramers);

        }


        void ISqler.Execute(string sqlText, DataReader process, params object[] paramers)
        {
            Progress(cmd =>
            {
                var dr = cmd.ExecuteReader();
                try
                {
                    process(dr);
                }
                finally
                {
                    dr.Close();
                }


                return true;
            }, sqlText, paramers);
        }



        DataTable ISqler.ExecuteTable(string sqlText, params object[] paramers)
        {
            return (DataTable)Progress(cmd =>
            {
                var d = dbProvider.DbFactory.CreateDataAdapter();
                try
                {
                    d.SelectCommand = cmd;
                    System.Data.DataTable tab = new DataTable();
                    d.Fill(tab);
                    return tab;
                }
                finally
                {
                    d.Dispose();
                }
            }, sqlText, paramers);
        }
        DataSet ISqler.ExecuteDataSet(string sqlText, params object[] paramers)
        {
            return (DataSet)Progress(cmd =>
            {
                var d = dbProvider.DbFactory.CreateDataAdapter();
                try
                {
                    d.SelectCommand = cmd;
                    System.Data.DataSet ds = new DataSet();
                    d.Fill(ds);
                    return ds;
                }
                finally
                {
                    d.Dispose();
                }
            }, sqlText, paramers);

        }

        T ISqler.ExecuteSingle<T>(string sqlText, params object[] paramers)
        {

            EntityHelper sqlHelper = new EntityHelper(this.dbProvider, typeof(T));
            return (T)Progress(cmd =>
            {
                IDataReader dr = cmd.ExecuteReader();
                try
                {
                    if (dr.Read())
                        return (T)CBO.CreateObject(Activator.CreateInstance(sqlHelper.ObjType), sqlHelper, dr);
                }
                finally
                {
                    dr.Close();
                }
                return null;
            }, sqlText, paramers);
        }

        List<T> ISqler.Execute<T>(string sqlText, params object[] paramers)
        {
            EntityHelper sqlHelper = new EntityHelper(this.dbProvider, typeof(T));
            var list = new List<T>();
            ISqler sqler = this;
            sqler.Execute<T>(sqlText, r => list.Add(r), paramers);
            return list;
        }

        IDictionary ISqler.ExecuteSingle(string sqlText, params object[] paramers)
        {

            return (IDictionary)Progress(cmd =>
            {
                IDataReader dr = cmd.ExecuteReader();
                IDictionary dic = new SortedList();
                try
                {
                    if (dr.Read())
                    {
                        for (int i = 0; i < dr.FieldCount; i++)
                            dic[dr.GetName(i)] = dr.GetValue(i);
                        return dic;
                    }
                    return null;
                }
                finally
                {
                    dr.Close();
                }
            }, sqlText, paramers);
        }






        DataTable ISqler.Execute(string sqlText, int start, int limit, params object[] paramers)
        {
            var sl = sqlText.Length;
            sqlText = dbProvider.GetPaginationText(start, limit, sqlText);
            var isP = sl == sqlText.Length;
            return (DataTable)Progress(cmd =>
            {
                var d = dbProvider.DbFactory.CreateDataAdapter();
                try
                {
                    d.SelectCommand = cmd;
                    System.Data.DataTable tab = new DataTable();
                    if (isP)
                    {
                        d.Fill(start, limit, tab);
                    }
                    else
                    {
                        d.Fill(tab);
                    }
                    return tab;
                }
                finally
                {
                    d.Dispose();
                }
            }, sqlText, paramers);
        }




        int ISqler.ExecuteNonQuery(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).ExecuteNonQuery(sqlText, new object[] { dictionary });

        }

        object ISqler.ExecuteScalar(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).ExecuteScalar(sqlText, new object[] { dictionary });
        }

        DataTable ISqler.ExecuteTable(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).ExecuteTable(sqlText, new object[] { dictionary });
        }

        DataTable ISqler.Execute(string sqlText, int start, int limit, IDictionary dictionary)
        {
            return ((ISqler)this).Execute(sqlText, start, limit, new object[] { dictionary });
        }

        DataSet ISqler.ExecuteDataSet(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).ExecuteDataSet(sqlText, new object[] { dictionary });
        }

        IDictionary ISqler.ExecuteSingle(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).ExecuteSingle(sqlText, new object[] { dictionary });
        }

        T ISqler.ExecuteSingle<T>(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).ExecuteSingle<T>(sqlText, new object[] { dictionary });
        }

        List<T> ISqler.Execute<T>(string sqlText, IDictionary dictionary)
        {
            return ((ISqler)this).Execute<T>(sqlText, new object[] { dictionary });
        }

        void ISqler.Execute(string sqlText, DataReader process, IDictionary dictionary)
        {
            ((ISqler)this).Execute(sqlText, process, new object[] { dictionary });
        }

        //static string Format(string format, List<object> olist, System.Collections.IDictionary diction)
        //{

        //    return DbCommonFactory.SqlFormat(format, olist, diction);

        //}



        List<T> ISqler.Execute<T>(string sqlText, int start, int limit, params object[] paramers)
        {
            var sL = sqlText.Length;
            sqlText = dbProvider.GetPaginationText(start, limit, sqlText);
            var isPag = sL == sqlText.Length;
            return (List<T>)Progress(cmd =>
            {
                EntityHelper sqlHelper = new EntityHelper(this.dbProvider, typeof(T));
                IDataReader dr = cmd.ExecuteReader();

                List<T> list = new List<T>();
                var i = 0;
                while (dr.Read())
                {
                    if (isPag)
                    {
                        if (start <= i)
                        {
                            list.Add((T)CBO.CreateObject(Activator.CreateInstance(sqlHelper.ObjType), sqlHelper, dr));
                        }
                        i++;
                        if (limit <= list.Count)
                        {
                            break;
                        }
                    }
                    else
                    {

                        list.Add((T)CBO.CreateObject(Activator.CreateInstance(sqlHelper.ObjType), sqlHelper, dr));
                    }
                }
                dr.Close();
                return list;
            }, sqlText, paramers);
        }

        List<T> ISqler.Execute<T>(string sqlText, int start, int limit, IDictionary paramKeys)
        {
            return ((ISqler)this).Execute<T>(sqlText, start, limit, new object[] { paramKeys });
        }



        int[] ISqler.ExecuteNonQuery(IDictionary paramKeys, params string[] sqlTexts)
        {
            var list = new List<int>();
            this.Command(cmd =>
            {
                for (var i = 0; i < sqlTexts.Length; i++)
                {
                    var sqlText = sqlTexts[i];

                    try
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = SqlParamer.Format(this.dbProvider, this.isAutoPfx, cmd, sqlText, paramKeys);
                        list.Add(cmd.ExecuteNonQuery());
                    }
                    catch
                    {
                        list.Add(-1);
                    }

                }
                return 0;

            });
            return list.ToArray();
        }



        void ISqler.Execute<T>(string sqlText, DataReader<T> reader, params object[] paramers)
        {

            EntityHelper sqlHelper = new EntityHelper(this.dbProvider, typeof(T));
            Progress(cmd =>
            {
                var dr = cmd.ExecuteReader();
                try
                {
                    while (dr.Read())
                        reader((T)CBO.CreateObject(Activator.CreateInstance(sqlHelper.ObjType), sqlHelper, dr));
                }
                finally
                {
                    dr.Close();
                }
                return 0;
            }, sqlText, paramers);
        }


        #region IScript Members
        Script script;
        Script IScript.SQL
        {
            get { return this.script; }
        }


        void ISqler.Execute<T>(string sqlText, int start, int limit, DataReader<T> reader, params object[] paramers)
        {
            var sL = sqlText.Length;
            sqlText = dbProvider.GetPaginationText(start, limit, sqlText);
            var isPag = sL == sqlText.Length;
            Progress(cmd =>
             {
                 EntityHelper sqlHelper = new EntityHelper(this.dbProvider, typeof(T));
                 IDataReader dr = cmd.ExecuteReader();
                 var Count = 0;
                 var i = 0;
                 while (dr.Read())
                 {
                     if (isPag)
                     {
                         if (start <= i)
                         {
                             Count++;
                             reader((T)CBO.CreateObject(Activator.CreateInstance(sqlHelper.ObjType), sqlHelper, dr));
                         }
                         i++;
                         if (limit <= Count)
                         {
                             break;
                         }
                     }
                     else
                     {

                         Count++;
                         reader((T)CBO.CreateObject(Activator.CreateInstance(sqlHelper.ObjType), sqlHelper, dr));
                     }
                 }
                 dr.Close();
                 return 0;
             }, sqlText, paramers);

        }

        void ISqler.Execute(System.Predicate<Script> predicate, System.Action<DbCommand> action)
        {
            this.Command(cmd =>
            {
                this.script = Script.Create(String.Empty, new object[0]);
                while (predicate(this.script))
                {
                    if (String.IsNullOrEmpty(this.script.Text))
                    {
                        break;
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = SqlParamer.Format(this.dbProvider, this.isAutoPfx, cmd, this.script.Text, this.script.Arguments);
                        action(cmd);
                        this.script.Reset(String.Empty);
                    }
                }
                return 0;
            });

        }


        int[] ISqler.ExecuteNonQuery(params Script[] scripts)
        {
            var l = -1;
            ISqler d = this;
            var list = new List<int>();


            d.Execute(sc =>
           {
               if (l < scripts.Length)
               {
                   sc.Reset(scripts[l].Text, scripts[l].Arguments);
                   return true;
               }
               return false;
           }, cmd => list.Add(cmd.ExecuteNonQuery()));
            return list.ToArray();
        }

        void ISqler.Execute(string sqlText, int start, int limit, DataRecord reader, params object[] paramers)
        {
            var sL = sqlText.Length;
            sqlText = dbProvider.GetPaginationText(start, limit, sqlText);
            var isPag = sL == sqlText.Length;
            Progress(cmd =>
            {
                IDataReader dr = cmd.ExecuteReader();

                var i = 0;
                var Count = 0;
                while (dr.Read())
                {
                    if (isPag)
                    {
                        if (start >= i)
                        {
                            Count++;
                            reader(dr);
                        }
                        i++;
                        if (limit <= Count)
                        {
                            break;
                        }
                    }
                    else
                    {

                        Count++;
                        reader(dr);
                    }
                }
                dr.Close();

                return Count;
            }, sqlText, paramers);
        }

        void ISqler.Execute(string sqlText, int start, int limit, DataRecord reader, IDictionary paramKeys)
        {

            ((ISqler)this).ExecuteNonQuery(sqlText, start, limit, reader, new object[] { paramKeys });
        }

        #endregion
    }
}
