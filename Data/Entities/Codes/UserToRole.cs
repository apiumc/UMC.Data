using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class UserToRole
    {
        readonly static Action<UserToRole, object>[] _SetValues = new Action<UserToRole, object>[] { (r, t) => r.Rolename = Reflection.ParseObject(t, r.Rolename), (r, t) => r.Site = Reflection.ParseObject(t, r.Site), (r, t) => r.user_id = Reflection.ParseObject(t, r.user_id) };
        readonly static string[] _Columns = new string[] { "Rolename", "Site", "user_id" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Rolename", this.Rolename);
            AppendValue(action, "Site", this.Site);
            AppendValue(action, "user_id", this.user_id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[3];
            cols[0] = RecordColumn.Column("Rolename", this.Rolename);
            cols[1] = RecordColumn.Column("Site", this.Site);
            cols[2] = RecordColumn.Column("user_id", this.user_id);
            return cols;
        }

    }
}

