using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace UMC.Data.Sql
{
    class Sequencer
    {
        private List<SequenceValue> list;

        public Sequencer()
        {
            list = new List<SequenceValue>();

        }
        Sequencer Order(string protortypeName, Sequence view)
        {
            SequenceValue compost = new SequenceValue { Name = protortypeName, Value = view };
            list.Add(compost);
            return this;
        }

        public void FormatSqlText(StringBuilder sb)
        {
            if (list.Count > 0)
            {
                sb.Append(" ORDER BY ");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.AppendFormat(" {0} {1}", list[i].Name, list[i].Value);
                    }
                    else
                    {
                        sb.AppendFormat(" ,{0} {1}", list[i].Name, list[i].Value);
                    }
                }
            }
        }




        class SequenceValue
        {
            public string Name;
            public Sequence Value;
        }


        #region ISequence Members

        public Sequencer Desc(string fieldName)
        {
            return this.Order(fieldName, Sequence.Desc);
        }

        public Sequencer Asc(string fieldName)
        {
            return this.Order(fieldName, Sequence.Asc);
        }

        public Sequencer Clear()
        {
            list.Clear();
            return this;
        }
        #endregion
        enum Sequence
        {
            Desc = 1,
            Asc = 0,
        }
    }
    class GSequencer<T> : IGroupOrder<T> where T : class
    {
        IGrouper<T> entity;
        Sequencer sequencer;
        List<string> fields = new List<string>();
        public GSequencer(IGrouper<T> entity)
        {
            this.sequencer = new Sequencer(); ;
            this.entity = entity;
            var cache = typeof(T).GetProperties();
            foreach (var info in cache)
            {
                if (info.CanRead)
                {
                    fields.Add(info.Name);
                }

            }
        }
        public void FormatSqlText(StringBuilder sb)
        {
            sequencer.FormatSqlText(sb);
        }


        IGroupOrder<T> IGroupOrder<T>.Desc(string fieldName)
        {
            //sequencer./
            if (fields.Exists(f => String.Equals(fieldName, f, StringComparison.CurrentCultureIgnoreCase)))
                sequencer.Desc(fieldName);
            return this;
        }

        IGroupOrder<T> IGroupOrder<T>.Asc(string fieldName)
        {
            if (fields.Exists(f => String.Equals(fieldName, f, StringComparison.CurrentCultureIgnoreCase)))
                sequencer.Asc(fieldName);
            return this;
        }

        IGroupOrder<T> IGroupOrder<T>.Clear()
        {
            sequencer.Clear();
            return this;
        }

        IGroupOrder<T> IGroupOrder<T>.Asc(T field)
        {
            //IGroupOrder<T> me = this;

            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                sequencer.Asc(em.Current.Key);
            }
            return this;
        }

        IGroupOrder<T> IGroupOrder<T>.Desc(T field)
        {
            //IGroupOrder<T> me = this;

            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                sequencer.Desc(em.Current.Key);
            }
            return this;
        }


        IGrouper<T> IGroupOrder<T>.Entities => this.entity;//throw new NotImplementedException();
    }
    class Sequencer<T> : IOrder<T> where T : class
    {
        IObjectEntity<T> entity;
        Sequencer sequencer;
        List<string> fields = new List<string>();
        public void FormatSqlText(StringBuilder sb)
        {
            sequencer.FormatSqlText(sb);
        }

        public Sequencer(IObjectEntity<T> entity)
        {
            this.sequencer = new Sequencer();
            this.entity = entity;
            var cache = typeof(T).GetProperties();
            foreach (var info in cache)
            {
                if (info.CanRead)
                {
                    fields.Add(info.Name);
                }

            }
            //entity.g
        }

        public IOrder<T> Asc(T field)
        {
            IOrder<T> me = this;

            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                me.Asc(em.Current.Key);
            }
            return me;
        }


        IOrder<T> IOrder<T>.Desc(string fieldName)
        {
            if (fields.Exists(f => String.Equals(fieldName, f, StringComparison.CurrentCultureIgnoreCase)))
                sequencer.Desc(fieldName);
            return this;
        }

        IOrder<T> IOrder<T>.Asc(string fieldName)
        {
            if (fields.Exists(f => String.Equals(fieldName, f, StringComparison.CurrentCultureIgnoreCase)))
                sequencer.Asc(fieldName);
            return this;
        }

        IOrder<T> IOrder<T>.Clear()
        {
            sequencer.Clear();
            return this;
        }

        IOrder<T> IOrder<T>.Asc(T field)
        {
            //IOrder<T> me = this;/

            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                sequencer.Asc(em.Current.Key);
            }
            return this;
        }

        IOrder<T> IOrder<T>.Desc(T field)
        {
            //IOrder<T> me = this;/

            var dic = CBO.GetProperty(field);
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                sequencer.Desc(em.Current.Key);
            }
            return this;
        }

        IObjectEntity<T> IOrder<T>.Entities => this.entity; // throw new NotImplementedException();


    }
}
