using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Role
    {
        readonly static Action<Role, object>[] _SetValues = new Action<Role, object>[] { (r, t) => r.Explain = Reflection.ParseObject(t, r.Explain), (r, t) => r.Rolename = Reflection.ParseObject(t, r.Rolename), (r, t) => r.Site = Reflection.ParseObject(t, r.Site) };
        readonly static string[] _Columns = new string[] { "Explain", "Rolename", "Site" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Explain", this.Explain);
            AppendValue(action, "Rolename", this.Rolename);
            AppendValue(action, "Site", this.Site);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[3];
            cols[0] = RecordColumn.Column("Explain", this.Explain);
            cols[1] = RecordColumn.Column("Rolename", this.Rolename);
            cols[2] = RecordColumn.Column("Site", this.Site);
            return cols;
        }

    }
}

