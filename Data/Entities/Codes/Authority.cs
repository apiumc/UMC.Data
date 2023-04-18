using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Authority
    {
        readonly static Action<Authority, object>[] _SetValues = new Action<Authority, object>[] { (r, t) => r.Body = Reflection.ParseObject(t, r.Body), (r, t) => r.Desc = Reflection.ParseObject(t, r.Desc), (r, t) => r.Key = Reflection.ParseObject(t, r.Key), (r, t) => r.Site = Reflection.ParseObject(t, r.Site) };
        readonly static string[] _Columns = new string[] { "Body", "Desc", "Key", "Site" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Body", this.Body);
            AppendValue(action, "Desc", this.Desc);
            AppendValue(action, "Key", this.Key);
            AppendValue(action, "Site", this.Site);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[4];
            cols[0] = RecordColumn.Column("Body", this.Body);
            cols[1] = RecordColumn.Column("Desc", this.Desc);
            cols[2] = RecordColumn.Column("Key", this.Key);
            cols[3] = RecordColumn.Column("Site", this.Site);
            return cols;
        }

    }
}

