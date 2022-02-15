using System;
using System.Collections.Generic;
using System.Text;
//using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.Common;

namespace UMC.Data.Sql
{
    class ObjectEntity<T> : IObjectEntity<T> where T : class
    {
        Conditions<T> cond;
        DbProvider db;
        Sequencer<T> seq;
        EntityHelper SqlHelper;
        ISqler sqler;
        public ObjectEntity(Sqler sqler, string tableName)
        {
            this.db = sqler.DbProvider;
            this.cond = new Conditions<T>(this);
            this.seq = new Sequencer<T>(this);
            this.SqlHelper = new EntityHelper(this.db, typeof(T), tableName);
            this.sqler = sqler;
        }

        object Run(string fnName, string field)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT {2}({1}) FROM {0} ", this.SqlHelper.TableInfo.Name, field, fnName);
            var li = cond.Wherer.FormatSqlText(sb, new List<object>(), this.db);

            this.script = Script.Create(sb.ToString(), li.ToArray());
            return this.sqler.ExecuteScalar(this.script.Text, this.script.Arguments);
        }
        T Run(string fnName, T field)
        {
            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            var v = false;

            while (em.MoveNext())
            {
                if (v)
                {
                    sb.Append(',');
                }
                else
                {
                    v = true;
                }
                sb.Append(fnName);
                sb.AppendFormat("({1}{0}{2}) AS {1}{0}{2}", em.Current.Key, this.db.QuotePrefix, this.db.QuoteSuffix);
                //me.Asc(em.Current.Key);
            }
            sb.AppendFormat(" FROM {0}", this.SqlHelper.TableInfo.Name);
            if (v)
            {    //sb.AppendFormat("SELECT {2}({1}) FROM {0} ", this.SqlHelper.TableInfo.Name, field, fnName);
                var li = cond.Wherer.FormatSqlText(sb, new List<object>(), this.db);

                this.script = Script.Create(sb.ToString(), li.ToArray());

                return this.sqler.ExecuteSingle<T>(this.script.Text, this.script.Arguments);
            }
            return default(T);
        }

        #region ISelect<T> Members

        T[] IObjectEntity<T>.Query()
        {
            IObjectEntity<T> d = this;
            return d.Query(default(T));
        }

        T[] IObjectEntity<T>.Query(int start, int limit)
        {

            IObjectEntity<T> d = this;
            return d.Query(default(T), start, limit);
        }

        T IObjectEntity<T>.Single()
        {
            IObjectEntity<T> d = this;
            return d.Single(default(T));

        }

        int IObjectEntity<T>.Count()
        {

            object v = Run("COUNT", "*");
            if (v == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(v);
            }
        }

        object IObjectEntity<T>.Sum(string field)
        {
            return Run("SUM", field);
        }

        object IObjectEntity<T>.Avg(string field)
        {
            return Run("AVG", field);
        }

        object IObjectEntity<T>.Max(string field)
        {
            return Run("MAX", field);
        }

        object IObjectEntity<T>.Min(string field)
        {
            return Run("MIN", field);
        }

        IOrder<T> IObjectEntity<T>.Order
        {
            get { return seq; }
        }

        int IObjectEntity<T>.Update(T item)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateUpdateText(String.Empty, item));
            List<object> list = new List<object>(this.SqlHelper.Arguments);

            cond.FormatSqlText(sb, list, this.db);

            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);

        }

        int IObjectEntity<T>.Delete()
        {
            var sb = new StringBuilder(this.SqlHelper.CreateDeleteText());
            var list = cond.FormatSqlText(sb, new List<object>(), this.db);
            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);

        }


        int IObjectEntity<T>.Insert(params T[] items)
        {
            if (items.Length > 0)
            {
                var feld = this.SqlHelper.Fields.Find(f => f.Attribute.AutoField);
                var em = items.GetEnumerator();
                List<object> list = new List<object>();
                if (feld == null)
                {

                    this.sqler.Execute(script =>
                    {
                        if (em.MoveNext())
                        {
                            var sqlText = this.SqlHelper.CreateInsertText(em.Current);

                            script.Reset(sqlText, this.SqlHelper.Arguments.ToArray());
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }, cmd => cmd.ExecuteNonQuery());
                }
                else
                {
                    this.sqler.Execute(script =>
                    {
                        if (em.MoveNext())
                        {
                            var sqlText = this.SqlHelper.CreateInsertText(em.Current);
                            sqlText = "\r\n" + this.db.GetIdentityText(this.SqlHelper.TableInfo.Name);

                            script.Reset(sqlText, this.SqlHelper.Arguments.ToArray());
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }, cmd => feld.Property.SetValue(em.Current, cmd.ExecuteScalar(), null));
                }
            }

            return items.Length;
        }



        object IObjectEntity<T>.Single(string field)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT {1} FROM {0} ", this.SqlHelper.TableInfo.Name, field);
            var li = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), li.ToArray());
            if (field.Trim() == "*")
            {
                return this.sqler.ExecuteSingle(this.script.Text, this.script.Arguments);
            }
            else
            {
                return this.sqler.ExecuteScalar(this.script.Text, this.script.Arguments);
            }
        }

        IDictionary IObjectEntity<T>.Single(params string[] field)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT {1} FROM {0} ", this.SqlHelper.TableInfo.Name, String.Join(",", field));
            var li = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), li.ToArray());

            return this.sqler.ExecuteSingle(this.script.Text, this.script.Arguments);

        }



        object[] IObjectEntity<T>.Query(string field)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT {1} FROM {0} ", this.SqlHelper.TableInfo.Name, field);
            var args = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), args.ToArray());
            if (field.Trim() == "*")
            {
                var data = sqler.ExecuteTable(this.script.Text, this.script.Arguments);
                var rows = new DataRow[data.Rows.Count];
                data.Rows.CopyTo(rows, 0);
                return rows;
            }
            var objs = new List<object>();
            this.sqler.Execute(this.script.Text, dr =>
            {
                try
                {
                    while (dr.Read())
                    {
                        objs.Add(dr[0]);
                    }
                }
                finally
                {
                    dr.Close();
                }
            }, this.script.Arguments);
            return objs.ToArray();

        }


        IWhere<T> IObjectEntity<T>.Where
        {
            get { return this.cond; }
        }



        T[] IObjectEntity<T>.Query(T item)
        {
            IObjectEntity<T> d = this;
            List<T> list = new List<T>();
            d.Query(item, rd => list.Add(rd));
            return list.ToArray();
        }

        T[] IObjectEntity<T>.Query(T item, int start, int limit)
        {
            if (limit <= 1)
            {
                throw new ArgumentException("limit必须>1");
            }
            if (start < 0)
            {
                throw new ArgumentException("start必须不小于0");
            }
            var sb = new StringBuilder(this.SqlHelper.CreateSelectText(item));
            var lp = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), lp.ToArray());
            return this.sqler.Execute<T>(this.script.Text, start, limit, this.script.Arguments).ToArray();
        }

        T IObjectEntity<T>.Single(T item)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateSelectText(item));
            var lp = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), lp.ToArray());
            return this.sqler.ExecuteSingle<T>(this.script.Text, this.script.Arguments);
        }



        int IObjectEntity<T>.Update(string format, T item)
        {

            var sb = new StringBuilder(this.SqlHelper.CreateUpdateText(format, item));
            List<object> list = new List<object>(this.SqlHelper.Arguments);

            cond.FormatSqlText(sb, list, this.db);

            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);
        }



        void IObjectEntity<T>.Query(DataReader<T> reader)
        {
            IObjectEntity<T> d = this;
            d.Query(default(T), reader);
        }

        void IObjectEntity<T>.Query(T item, DataReader<T> reader)
        {
            var sb = new StringBuilder();
            sb.Append(this.SqlHelper.CreateSelectText(item));
            var args = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), args.ToArray());
            this.sqler.Execute<T>(this.script.Text, reader, this.script.Arguments);
        }
        void IObjectEntity<T>.Distinct(T item, DataReader<T> reader)
        {
            var sb = new StringBuilder();
            sb.Append(this.SqlHelper.CreateSelectText(item));
            var args = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            sb.Insert(7, "Distinct ");
            this.script = Script.Create(sb.ToString(), args.ToArray());
            this.sqler.Execute<T>(this.script.Text, reader, this.script.Arguments);
        }


        int IObjectEntity<T>.Update(IDictionary fieldValues)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateUpdateText(String.Empty, fieldValues));
            List<object> list = new List<object>(this.SqlHelper.Arguments);

            cond.FormatSqlText(sb, list, this.db);

            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);

        }

        int IObjectEntity<T>.Update(string format, IDictionary fieldValues)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateUpdateText(format, fieldValues));
            List<object> list = new List<object>(this.SqlHelper.Arguments);

            cond.FormatSqlText(sb, list, this.db);

            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);
        }


        IGrouper<T> IObjectEntity<T>.GroupBy(params string[] fields)
        {
            return new Grouper<T>((Sqler)this.sqler, this.SqlHelper, this.cond, fields);
        }

        IGrouper<T> IObjectEntity<T>.GroupBy(T field)
        {
            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            var fields = new List<String>();
            while (em.MoveNext())
            {
                fields.Add(em.Current.Key);
            }
            return new Grouper<T>((Sqler)this.sqler, this.SqlHelper, this.cond, fields.ToArray());
        }


        Script script;
        Script IScript.SQL
        {
            get { return this.script; }
        }



        Script IObjectEntity<T>.Script(T field)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateSelectText(field));
            var lp = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            return Script.Create(sb.ToString(), lp.ToArray());
        }

        Script IObjectEntity<T>.Script(string field)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT {1} FROM {0} ", this.SqlHelper.TableInfo.Name, field);
            var lp = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            return Script.Create(sb.ToString(), lp.ToArray());
        }



        void IObjectEntity<T>.Query(int start, int limit, DataReader<T> dr)
        {

            IObjectEntity<T> d = this;
            d.Query(default(T), start, limit, dr);
        }

        void IObjectEntity<T>.Query(T field, int start, int limit, DataReader<T> dr)
        {
            if (limit <= 1)
            {
                throw new ArgumentException("limit必须>1");
            }
            if (start < 0)
            {
                throw new ArgumentException("start必须不小于0");
            }
            var sb = new StringBuilder(this.SqlHelper.CreateSelectText(field));
            var lp = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), lp.ToArray());
            this.sqler.Execute<T>(this.script.Text, start, limit, dr, this.script.Arguments);
        }




        void IObjectEntity<T>.Query(string field, DataReader dr)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT {1} FROM {0} ", this.SqlHelper.TableInfo.Name, field);
            // this.sqler.Execute(this.script.Text, dr, this.script.Arguments);
            var lp = cond.FormatSqlText(sb, new List<object>(), this.db);
            seq.FormatSqlText(sb);
            this.script = Script.Create(sb.ToString(), lp.ToArray());
            this.sqler.Execute(this.script.Text, dr, this.script.Arguments);
        }
        T IObjectEntity<T>.Sum(T field)
        {
            return Run("SUM", field);
        }

        T IObjectEntity<T>.Avg(T field)
        {
            return Run("AVG", field);
        }

        T IObjectEntity<T>.Max(T field)
        {
            return Run("MAX", field);
        }

        T IObjectEntity<T>.Min(T field)
        {
            return Run("MIN", field);
        }

        #endregion

        #region IObjectEntity<T> Members


        IObjectEntity<T> IObjectEntity<T>.IFF(Predicate<IObjectEntity<T>> where, System.Action<IObjectEntity<T>> @true)
        {
            if (where(this))
            {
                if (@true != null) @true(this);
            }
            return this;
        }
        IObjectEntity<T> IObjectEntity<T>.IFF(Predicate<IObjectEntity<T>> where, System.Action<IObjectEntity<T>> @true, System.Action<IObjectEntity<T>> @false)
        {
            if (where(this))
            {
                if (@true != null) @true(this);
            }
            else
            {
                if (@false != null) @false(this);
            }
            return this;
        }

        #endregion


        void IObjectEntity<T>.AddField(string field, string name)
        {
            var cfield = this.SqlHelper.Fields.Find(g => g.Property.Name == name);
            if (cfield != null)
            {
                cfield.Attribute = new FieldAttribute(field, true) { Select = true };

            }
            else
            {
                this.SqlHelper.Fields.Add(new EntityHelper.FieldInfo
                {
                    Name = name,
                    Attribute = new FieldAttribute(field, true) { Select = true },
                    FieldIndex = this.SqlHelper.Fields.Count
                });

            }
        }

        int IObjectEntity<T>.Update(string format, T field, T field2)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateUpdateText(format, field));
            this.SqlHelper.AppendUpdateText(sb, field2, String.Empty);
            List<object> list = new List<object>(this.SqlHelper.Arguments);

            cond.FormatSqlText(sb, list, this.db);

            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);

        }

        int IObjectEntity<T>.Update(T field, T field2)
        {
            var sb = new StringBuilder(this.SqlHelper.CreateUpdateText("{0}+{1}", field));
            this.SqlHelper.AppendUpdateText(sb, field2, String.Empty);
            List<object> list = new List<object>(this.SqlHelper.Arguments);

            cond.FormatSqlText(sb, list, this.db);

            this.script = Script.Create(sb.ToString(), list.ToArray());
            return this.sqler.ExecuteNonQuery(this.script.Text, this.script.Arguments);
        }
    }
}
