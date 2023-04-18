using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Picture
    {
        readonly static Action<Picture, object>[] _SetValues = new Action<Picture, object>[] { (r, t) => r.group_id = Reflection.ParseObject(t, r.group_id), (r, t) => r.UploadTime = Reflection.ParseObject(t, r.UploadTime), (r, t) => r.user_id = Reflection.ParseObject(t, r.user_id), (r, t) => r.Value = Reflection.ParseObject(t, r.Value) };
        readonly static string[] _Columns = new string[] { "group_id", "UploadTime", "user_id", "Value" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "group_id", this.group_id);
            AppendValue(action, "UploadTime", this.UploadTime);
            AppendValue(action, "user_id", this.user_id);
            AppendValue(action, "Value", this.Value);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[4];
            cols[0] = RecordColumn.Column("group_id", this.group_id);
            cols[1] = RecordColumn.Column("UploadTime", this.UploadTime);
            cols[2] = RecordColumn.Column("user_id", this.user_id);
            cols[3] = RecordColumn.Column("Value", this.Value);
            return cols;
        }

    }
}

