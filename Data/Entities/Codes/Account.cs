using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Account
    {
        readonly static Action<Account, object>[] _SetValues = new Action<Account, object>[] { (r, t) => r.ConfigData = Reflection.ParseObject(t, r.ConfigData), (r, t) => r.Flags = Reflection.ParseObject(t, r.Flags), (r, t) => r.ForId = Reflection.ParseObject(t, r.ForId), (r, t) => r.ForKey = Reflection.ParseObject(t, r.ForKey), (r, t) => r.Name = Reflection.ParseObject(t, r.Name), (r, t) => r.Type = Reflection.ParseObject(t, r.Type), (r, t) => r.user_id = Reflection.ParseObject(t, r.user_id) };
        readonly static string[] _Columns = new string[] { "ConfigData", "Flags", "ForId", "ForKey", "Name", "Type", "user_id" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "ConfigData", this.ConfigData);
            AppendValue(action, "Flags", this.Flags);
            AppendValue(action, "ForId", this.ForId);
            AppendValue(action, "ForKey", this.ForKey);
            AppendValue(action, "Name", this.Name);
            AppendValue(action, "Type", this.Type);
            AppendValue(action, "user_id", this.user_id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[7];
            cols[0] = RecordColumn.Column("ConfigData", this.ConfigData);
            cols[1] = RecordColumn.Column("Flags", this.Flags);
            cols[2] = RecordColumn.Column("ForId", this.ForId);
            cols[3] = RecordColumn.Column("ForKey", this.ForKey);
            cols[4] = RecordColumn.Column("Name", this.Name);
            cols[5] = RecordColumn.Column("Type", this.Type);
            cols[6] = RecordColumn.Column("user_id", this.user_id);
            return cols;
        }

    }
}

