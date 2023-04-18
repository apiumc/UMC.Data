using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Password
    {
        readonly static Action<Password, object>[] _SetValues = new Action<Password, object>[] { (r, t) => r.Body = Reflection.ParseObject(t, r.Body), (r, t) => r.Key = Reflection.ParseObject(t, r.Key) };
        readonly static string[] _Columns = new string[] { "Body", "Key" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Body", this.Body);
            AppendValue(action, "Key", this.Key);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[2];
            cols[0] = RecordColumn.Column("Body", this.Body);
            cols[1] = RecordColumn.Column("Key", this.Key);
            return cols;
        }

    }
}

