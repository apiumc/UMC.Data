using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Menu
    {
        readonly static Action<Menu, object>[] _SetValues = new Action<Menu, object>[] { (r, t) => r.Caption = Reflection.ParseObject(t, r.Caption), (r, t) => r.Icon = Reflection.ParseObject(t, r.Icon), (r, t) => r.Id = Reflection.ParseObject(t, r.Id), (r, t) => r.IsHidden = Reflection.ParseObject(t, r.IsHidden), (r, t) => r.ParentId = Reflection.ParseObject(t, r.ParentId), (r, t) => r.Seq = Reflection.ParseObject(t, r.Seq), (r, t) => r.Site = Reflection.ParseObject(t, r.Site), (r, t) => r.Type = Reflection.ParseObject(t, r.Type), (r, t) => r.Url = Reflection.ParseObject(t, r.Url), (r, t) => r.Value = Reflection.ParseObject(t, r.Value) };
        readonly static string[] _Columns = new string[] { "Caption", "Icon", "Id", "IsHidden", "ParentId", "Seq", "Site", "Type", "Url", "Value" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "Caption", this.Caption);
            AppendValue(action, "Icon", this.Icon);
            AppendValue(action, "Id", this.Id);
            AppendValue(action, "IsHidden", this.IsHidden);
            AppendValue(action, "ParentId", this.ParentId);
            AppendValue(action, "Seq", this.Seq);
            AppendValue(action, "Site", this.Site);
            AppendValue(action, "Type", this.Type);
            AppendValue(action, "Url", this.Url);
            AppendValue(action, "Value", this.Value);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[10];
            cols[0] = RecordColumn.Column("Caption", this.Caption);
            cols[1] = RecordColumn.Column("Icon", this.Icon);
            cols[2] = RecordColumn.Column("Id", this.Id);
            cols[3] = RecordColumn.Column("IsHidden", this.IsHidden);
            cols[4] = RecordColumn.Column("ParentId", this.ParentId);
            cols[5] = RecordColumn.Column("Seq", this.Seq);
            cols[6] = RecordColumn.Column("Site", this.Site);
            cols[7] = RecordColumn.Column("Type", this.Type);
            cols[8] = RecordColumn.Column("Url", this.Url);
            cols[9] = RecordColumn.Column("Value", this.Value);
            return cols;
        }

    }
}

