using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class OrganizeMember
    {
        readonly static Action<OrganizeMember, object>[] _SetValues = new Action<OrganizeMember, object>[] { (r, t) => r.MemberType = Reflection.ParseObject(t, r.MemberType), (r, t) => r.ModifyTime = Reflection.ParseObject(t, r.ModifyTime), (r, t) => r.org_id = Reflection.ParseObject(t, r.org_id), (r, t) => r.Seq = Reflection.ParseObject(t, r.Seq), (r, t) => r.user_id = Reflection.ParseObject(t, r.user_id) };
        readonly static string[] _Columns = new string[] { "MemberType", "ModifyTime", "org_id", "Seq", "user_id" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "MemberType", this.MemberType);
            AppendValue(action, "ModifyTime", this.ModifyTime);
            AppendValue(action, "org_id", this.org_id);
            AppendValue(action, "Seq", this.Seq);
            AppendValue(action, "user_id", this.user_id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[5];
            cols[0] = RecordColumn.Column("MemberType", this.MemberType);
            cols[1] = RecordColumn.Column("ModifyTime", this.ModifyTime);
            cols[2] = RecordColumn.Column("org_id", this.org_id);
            cols[3] = RecordColumn.Column("Seq", this.Seq);
            cols[4] = RecordColumn.Column("user_id", this.user_id);
            return cols;
        }

    }
}

