using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Config
    {
        readonly static Action<Config, object>[] _SetValues = new Action<Config, object>[] { (r, t) => r.ConfKey = Reflection.ParseObject(t, r.ConfKey), (r, t) => r.ConfValue = Reflection.ParseObject(t, r.ConfValue) };
        readonly static string[] _Columns = new string[] { "ConfKey", "ConfValue" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "ConfKey", this.ConfKey);
            AppendValue(action, "ConfValue", this.ConfValue);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[2];
            cols[0] = RecordColumn.Column("ConfKey", this.ConfKey);
            cols[1] = RecordColumn.Column("ConfValue", this.ConfValue);
            return cols;
        }

    }
}

