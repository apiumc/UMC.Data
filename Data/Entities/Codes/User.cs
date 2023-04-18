using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class User
    {
        readonly static Action<User, object>[] _SetValues = new Action<User, object>[] { (r, t) => r.ActiveTime = Reflection.ParseObject(t, r.ActiveTime), (r, t) => r.Alias = Reflection.ParseObject(t, r.Alias), (r, t) => r.Flags = Reflection.ParseObject(t, r.Flags), (r, t) => r.Id = Reflection.ParseObject(t, r.Id), (r, t) => r.IsDisabled = Reflection.ParseObject(t, r.IsDisabled), (r, t) => r.RegistrTime = Reflection.ParseObject(t, r.RegistrTime), (r, t) => r.Signature = Reflection.ParseObject(t, r.Signature), (r, t) => r.Username = Reflection.ParseObject(t, r.Username), (r, t) => r.VerifyTimes = Reflection.ParseObject(t, r.VerifyTimes) };
        readonly static string[] _Columns = new string[] { "ActiveTime", "Alias", "Flags", "Id", "IsDisabled", "RegistrTime", "Signature", "Username", "VerifyTimes" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "ActiveTime", this.ActiveTime);
            AppendValue(action, "Alias", this.Alias);
            AppendValue(action, "Flags", this.Flags);
            AppendValue(action, "Id", this.Id);
            AppendValue(action, "IsDisabled", this.IsDisabled);
            AppendValue(action, "RegistrTime", this.RegistrTime);
            AppendValue(action, "Signature", this.Signature);
            AppendValue(action, "Username", this.Username);
            AppendValue(action, "VerifyTimes", this.VerifyTimes);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[9];
            cols[0] = RecordColumn.Column("ActiveTime", this.ActiveTime);
            cols[1] = RecordColumn.Column("Alias", this.Alias);
            cols[2] = RecordColumn.Column("Flags", this.Flags);
            cols[3] = RecordColumn.Column("Id", this.Id);
            cols[4] = RecordColumn.Column("IsDisabled", this.IsDisabled);
            cols[5] = RecordColumn.Column("RegistrTime", this.RegistrTime);
            cols[6] = RecordColumn.Column("Signature", this.Signature);
            cols[7] = RecordColumn.Column("Username", this.Username);
            cols[8] = RecordColumn.Column("VerifyTimes", this.VerifyTimes);
            return cols;
        }

    }
}

