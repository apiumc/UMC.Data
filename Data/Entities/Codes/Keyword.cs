using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Keyword
    {
        readonly static Action<Keyword, object>[] _SetValues = new Action<Keyword, object>[] { (r, t) => r.Key = Reflection.ParseObject(t, r.Key), (r, t) => r.Time = Reflection.ParseObject(t, r.Time), (r, t) => r.user_id = Reflection.ParseObject(t, r.user_id) };
        readonly static string[] _Columns = new string[] { "Key", "Time", "user_id" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Key", this.Key);
            AppendValue(action, "Time", this.Time);
            AppendValue(action, "user_id", this.user_id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[3];
            cols[0] = RecordColumn.Column("Key", this.Key);
            cols[1] = RecordColumn.Column("Time", this.Time);
            cols[2] = RecordColumn.Column("user_id", this.user_id);
            return cols;
        }

    }
}

