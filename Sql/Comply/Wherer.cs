using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace UMC.Data.Sql
{
    class Conditions<T> : IWhere<T> where T : class
    {
        public Conditions Wherer
        {
            get;
            set;
        }
        public List<object> FormatSqlText(StringBuilder sb, List<object> lp,DbProvider dbProvider)
        {

            return Wherer.FormatSqlText(sb, lp, dbProvider);
        }
        public Conditions(IObjectEntity<T> entity)
        {
            this.Wherer = new Conditions();
            this.IWherer = this.Wherer; ;
            this.entity = entity;
        }
        IObjectEntity<T> entity;
        public Conditions(Conditions con, IObjectEntity<T> entity)
        {
            this.Wherer = con;
            this.IWherer = con;
            this.entity = entity;
        }
        Conditions IWherer;



        #region IConstraint<T> Members
        IWhere<T> IWhere<T>.Reset()
        {
            IWherer.Reset();
            return this;
        }

        int IWhere<T>.Remove(string name)
        {
            return IWherer.Remove(name);
        }

        IWhere<T> IWhere<T>.Or(string expression, params object[] paramers)
        {
            this.IWherer.Or(expression, paramers);
            return this;
        }

        IWhere<T> IWhere<T>.And(string expression, params object[] paramers)
        {
            this.IWherer.And(expression, paramers);
            return this;
        }

        IOperator<T> IWhere<T>.Or()
        {
            return new Operator<T>(new Operator(this.Wherer, true), this);
        }

        IOperator<T> IWhere<T>.And()
        {
            return new Operator<T>(new Operator(this.Wherer, false), this);
        }

        IWhere<T> IWhere<T>.Or(T item)
        {
            var dic = CBO.GetProperty(item);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                this.IWherer.Or().Equal(em.Current.Key, em.Current.Value);
            }
            return this;
        }

        IWhere<T> IWhere<T>.And(T item)
        {
            var dic = CBO.GetProperty(item);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                this.IWherer.And().Equal(em.Current.Key, em.Current.Value);
            }
            return this;

        }

        IWhere<T> IWhere<T>.Remove(T item)
        {
            var dic = CBO.GetProperty(item);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                this.IWherer.Remove(em.Current.Key);
            }
            return this;
        }

        #endregion

        #region IConstraint<T> Members


        IWhere<T> IWhere<T>.Replace(T field)
        {
            //this.Condit.
            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                if (this.Wherer.ContainsKey(em.Current.Key))
                {
                    IWherer[em.Current.Key] = em.Current.Value;
                }
            }
            return this;

        }

        #endregion

        #region IConstraint<T> Members

        IObjectEntity<T> IWhere<T>.Entities
        {
            get { return this.entity; }
        }


        #endregion

        #region IWhere Members


        object IWhere<T>.this[string name]
        {
            get
            {
                return this.IWherer[name];
            }
            set
            {
                this.IWherer[name] = value;
            }
        }


        //IWhere IWhere.Reset()
        //{
        //    this.IWherer.Reset();
        //    return this;
        //}

        //int IWhere.Remove(string name)
        //{
        //    return this.IWherer.Remove(name);
        //}

        int IWhere<T>.Count
        {
            get { return this.IWherer.Count; }
        }



        //IWhere IWhere.Or(string expression, params object[] paramers)
        //{
        //    return this.IWherer.Or(expression, paramers);
        //}

        //IWhere IWhere.And(string expression, params object[] paramers)
        //{
        //    return this.IWherer.And(expression, paramers);
        //}

        //IOperator IWhere.Or()
        //{
        //    return this.IWherer.Or();//(expression, paramers);
        //}

        //IOperator IWhere.And()
        //{
        //    return this.IWherer.And();
        //}

        //IWhere IWhere.Contains()
        //{
        //    return this.IWherer.Contains();
        //}



        public IWhere<T> Contains()
        {
            var p = (Conditions)this.IWherer.Contains();

            return new Conditions<T>(p, this.entity);

        }



        //IWhere IWhere.Contains()
        //{
        //    return this.IWherer.Contains();
        //}

        #endregion
    }
    /// <summary>
    /// 查询条件
    /// </summary>
    class Conditions
    {
        public bool ContainsKey(string field)
        {
            return this.Arguments.Exists(a => String.Equals(a.PropertyName, field, StringComparison.CurrentCultureIgnoreCase));
        }

        public class FieldArgument
        {

            public Guid ForId { get; set; }
            public Guid GroubId { get; set; }
            public string PropertyName { get; set; }
            public object Value { get; set; }
            public DbOperator ConstraintVocable { get; set; }
            public JoinVocable JoinVocable { get; set; }
            public JoinVocable? FristJoin { get; set; }

        }
        public JoinVocable? FristJoin { get; set; }
        public enum JoinVocable
        {
            Empty = 0,
            And = 1,
            Or = 2
        }

        public Guid GroubId
        {
            get;
            set;
        }
        public List<FieldArgument> Arguments
        {
            get;
            set;
        }
        //public List<object> SqlParams = new List<object>();
        public Conditions(string field, DbOperator co, object value)
        {
            GroubId = Guid.NewGuid();
            Arguments = new List<FieldArgument>();
            FieldArgument pr = new FieldArgument();
            pr.PropertyName = field;
            pr.ConstraintVocable = co;
            pr.Value = value;
            pr.JoinVocable = JoinVocable.Empty;
            pr.GroubId = GroubId;
            Arguments.Add(pr);
        }
        public Conditions(JoinVocable json)
            : this()
        {
            FristJoin = json;
        }
        public Conditions()
        {
            GroubId = Guid.NewGuid();
            Arguments = new List<FieldArgument>();
        }
        public Conditions Reset()
        {
            Arguments.Clear();
            return this;
        }
        public Conditions Or(string expression, params object[] objs)
        {
            FieldArgument pr = new FieldArgument
            {
                ForId = GroubId,
                GroubId = Guid.NewGuid(),
                PropertyName = expression,
                ConstraintVocable = DbOperator.Expression,
                Value = objs,
                JoinVocable = JoinVocable.Or
            };
            this.CheckGroup(pr);

            return this;
        }

        public Conditions And(string expression, params object[] objs)
        {
            FieldArgument pr = new FieldArgument
            {
                ForId = GroubId,
                GroubId = Guid.NewGuid(),
                PropertyName = expression,
                ConstraintVocable = DbOperator.Expression,
                Value = objs,
                FristJoin = FristJoin,
                JoinVocable = JoinVocable.And
            };
            this.CheckGroup(pr);

            return this;
        }
        public Conditions And(string pName, DbOperator co, object value)
        {
            FieldArgument pr = new FieldArgument();
            pr.ForId = GroubId;
            pr.GroubId = Guid.NewGuid();
            pr.PropertyName = pName;
            pr.ConstraintVocable = co;
            pr.Value = value;
            pr.JoinVocable = JoinVocable.And;
            this.CheckGroup(pr);

            return this;
        }
        public Conditions Or(string pName, DbOperator co, object value)
        {
            FieldArgument pr = new FieldArgument();
            pr.ForId = this.GroubId;
            pr.GroubId = Guid.NewGuid();
            pr.PropertyName = pName;
            pr.ConstraintVocable = co;
            pr.Value = value;
            pr.JoinVocable = JoinVocable.Or;
            this.CheckGroup(pr);
            return this;
        }
        void CheckGroup(FieldArgument pr)
        {
            if (this.IsContainFrist)
            {
                this.GroubId = pr.GroubId;
                pr.FristJoin = FristJoin;
                this.IsContainFrist = false;
            }
            this.Arguments.Add(pr);
        }
        public bool IsContainFrist = false;


        string SqlFormat(string sqlText, object[] paramers, List<object> sqlParams)
        {
            var count = sqlParams.Count;
            sqlText = System.Text.RegularExpressions.Regex.Replace(sqlText, @"\{(\d+)\}", match =>
                      {
                          int index = Convert.ToInt32(match.Groups[1].Value);
                          if (index >= paramers.Length)
                          {
                              throw new System.ArgumentOutOfRangeException("sqlTex", "SQL参数超过了索引");
                          }
                          string str = String.Format("{{{0}}}", count + index);
                          return str;
                      });
            sqlParams.AddRange(paramers);
            return sqlText;

        }
        void AppendInParameter1(StringBuilder sb, List<object> sqlParams, Array values, int level)
        {
            bool isWrite = false;
            level++;
            for (var i = 0; i < values.Length; i++)
            {
                if (isWrite)
                {
                    sb.Append(',');
                }
                else
                {
                    isWrite = true;
                }
                var va = values.GetValue(i);
                if (level < 2)
                {
                    if (va != null && va.GetType().IsArray)
                    {
                        var vas = va as Array;
                        AppendInParameter1(sb, sqlParams, vas, level);
                        if (vas.Length == 0)
                        {
                            isWrite = false;
                        }
                        continue;
                    }
                }

                sb.Append('{');
                sb.Append(sqlParams.Count);
                sb.Append('}');

                sqlParams.Add(va);

            }
        }
        void AppendInParameter(StringBuilder sb, List<object> sqlParams, object value)
        {
            var type = value.GetType();
            if (type.IsArray)
            {
                this.AppendInParameter(sb, sqlParams, (object[])value);
            }
            else
            {
                this.AppendInParameter(sb, sqlParams, new object[] { value });
            }
        }
        void AppendInParameter(StringBuilder sb, List<object> sqlParams, Array values)
        {
            switch (values.Length)
            {
                case 1:
                    var v = values.GetValue(0);
                    if (v != null)
                    {
                        var type = v.GetType();
                        if (type.Equals(typeof(Script)))
                        {
                            var sciipt = v as Script;
                            sb.Append(this.SqlFormat(sciipt.Text, sciipt.Arguments, sqlParams));
                            break;
                        }
                    }
                    goto default;
                default:
                    AppendInParameter1(sb, sqlParams, values, 0);

                    break;
            }
        }
        private void SetParameter(DbProvider dbProvider, FieldArgument param, StringBuilder sb, List<object> sqlParams)
        {
            if (param.ConstraintVocable == DbOperator.Expression)
            {
                sb.Append(this.SqlFormat(param.PropertyName, (object[])param.Value, sqlParams));
                return;
            }

            string proKey = param.PropertyName;
            sb.Append(dbProvider.QuotePrefix);
            sb.Append(proKey);
            sb.Append(dbProvider.QuoteSuffix);
            switch (param.ConstraintVocable)
            {
                case DbOperator.Unequal:
                    sb.Append(" <>  ");
                    break;
                case DbOperator.Equal:
                    sb.Append(" =  ");
                    break;
                case DbOperator.Greater:
                    sb.Append(" >  ");
                    break;
                case DbOperator.GreaterEqual:
                    sb.Append(" >=  ");
                    break;
                case DbOperator.NotLike:
                    sb.Append(" NOT LIKE ");
                    break;
                case DbOperator.Like:
                    sb.Append(" LIKE  ");
                    break;
                case DbOperator.Smaller:
                    sb.Append(" <  ");
                    break;
                case DbOperator.SmallerEqual:
                    sb.Append(" <=  ");
                    break;
                case DbOperator.In:
                    sb.Append(" IN(");
                    AppendInParameter(sb, sqlParams, param.Value);
                    sb.Append(')');
                    return;
                case DbOperator.NotIn:
                    sb.Append(" NOT IN(");
                    AppendInParameter(sb, sqlParams, param.Value);
                    sb.Append(')');
                    return;
                default:
                    throw new System.ArgumentException("DbOperator 参数无效");
            }
            sb.AppendFormat(" {{{0}}} ", sqlParams.Count);
            sqlParams.Add(param.Value);

        }
        private void SetGroup(DbProvider dbProvider, int destLength, Guid forid, StringBuilder sb, List<object> sqlParams)
        {
            List<FieldArgument> parms = Arguments.FindAll(param => param.ForId == forid);
            for (int i = 0; i < parms.Count; i++)
            {
                if (Arguments.Exists(para => para.ForId == parms[i].GroubId))
                {
                    if (sb.Length == destLength)
                    {
                        sb.Append(" WHERE ( ");//, parms[i].JoinVocable);
                    }
                    else
                    {
                        var pjs = Arguments[Arguments.IndexOf(parms[i]) - 1];
                        sb.AppendFormat(" {0} ( ", parms[i].FristJoin ?? pjs.JoinVocable);
                    }

                    SetParameter(dbProvider,parms[i], sb, sqlParams);

                    SetGroup(dbProvider,destLength, parms[i].GroubId, sb, sqlParams);
                    sb.Append(") ");
                }
                else
                {
                    if (sb.Length == destLength)
                    {
                        sb.AppendFormat(" WHERE ", parms[i].JoinVocable);
                    }
                    else
                    {

                        sb.AppendFormat(" {0}  ", parms[i].JoinVocable);
                    }


                    SetParameter(dbProvider,parms[i], sb, sqlParams);

                }
            }
        }

        #region IConstraint Members

        public List<object> FormatSqlText(StringBuilder sb, List<object> lp, DbProvider dbProvider)
        {
            if (Arguments.Count >= 0)
            {
                //  sb.Append(" Where 1=1 ");
                SetGroup(dbProvider,sb.Length, this.GroubId, sb, lp);
            }
            return lp;
        }
        public Operator Or()
        {
            return new Operator(this, true);
        }

        public Operator And()
        {
            return new Operator(this, false);
        }


        public object this[string name]
        {
            get
            {
                var ps = this.Arguments.FindAll(p =>
                  {
                      return p.ConstraintVocable != DbOperator.Expression && p.ForId == this.GroubId && p.PropertyName == name;
                  });
                switch (ps.Count)
                {
                    case 1:
                        return ps[0].Value;
                    case 0:
                        throw new ArgumentException(String.Format("‘{0}’的字段不存在", name));
                    default:
                        throw new ArgumentException(String.Format("‘{0}’的字段有多个值", name));

                }
            }
            set
            {
                var ps = this.Arguments.FindAll(p =>
                {
                    return p.ConstraintVocable != DbOperator.Expression && p.ForId == this.GroubId && p.PropertyName == name;
                });

                switch (ps.Count)
                {
                    case 1:
                        ps[0].Value = value;
                        break;
                    case 0:
                        throw new ArgumentException(String.Format("‘{0}’的字段不存在", name));
                    default:
                        throw new ArgumentException(String.Format("‘{0}’的字段有多个值", name));


                }
            }
        }


        public int Remove(string name)
        {
            return this.Arguments.RemoveAll(p =>
             {
                 return p.ConstraintVocable != DbOperator.Expression && p.ForId == this.GroubId && p.PropertyName == name;
             });

        }


        public int Count
        {
            get
            {
                return this.Arguments.FindAll(p => p.ForId == this.GroubId).Count;
            }
        }
        public Conditions Contains()
        {
            var query = new Conditions();
            query.GroubId = this.GroubId;
            query.IsContainFrist = true;
            query.Arguments = this.Arguments;
            return query;
        }

        #endregion
    }
}
