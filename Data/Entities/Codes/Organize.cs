using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Organize
    {
        readonly static Action<Organize, object>[] _SetValues = new Action<Organize, object>[] { (r, t) => r.Caption = Reflection.ParseObject(t, r.Caption), (r, t) => r.Id = Reflection.ParseObject(t, r.Id), (r, t) => r.Identifier = Reflection.ParseObject(t, r.Identifier), (r, t) => r.Members = Reflection.ParseObject(t, r.Members), (r, t) => r.ModifyTime = Reflection.ParseObject(t, r.ModifyTime), (r, t) => r.ParentId = Reflection.ParseObject(t, r.ParentId), (r, t) => r.Seq = Reflection.ParseObject(t, r.Seq) };
        readonly static string[] _Columns = new string[] { "Caption", "Id", "Identifier", "Members", "ModifyTime", "ParentId", "Seq" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Caption", this.Caption);
            AppendValue(action, "Id", this.Id);
            AppendValue(action, "Identifier", this.Identifier);
            AppendValue(action, "Members", this.Members);
            AppendValue(action, "ModifyTime", this.ModifyTime);
            AppendValue(action, "ParentId", this.ParentId);
            AppendValue(action, "Seq", this.Seq);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[7];
            cols[0] = RecordColumn.Column("Caption", this.Caption);
            cols[1] = RecordColumn.Column("Id", this.Id);
            cols[2] = RecordColumn.Column("Identifier", this.Identifier);
            cols[3] = RecordColumn.Column("Members", this.Members);
            cols[4] = RecordColumn.Column("ModifyTime", this.ModifyTime);
            cols[5] = RecordColumn.Column("ParentId", this.ParentId);
            cols[6] = RecordColumn.Column("Seq", this.Seq);
            return cols;
        }

    }
}

