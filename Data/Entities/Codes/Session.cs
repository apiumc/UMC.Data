using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Session
    {
        readonly static Action<Session, object>[] _SetValues = new Action<Session, object>[] { (r, t) => r.ClientIP = Reflection.ParseObject(t, r.ClientIP), (r, t) => r.Content = Reflection.ParseObject(t, r.Content), (r, t) => r.ContentType = Reflection.ParseObject(t, r.ContentType), (r, t) => r.DeviceToken = Reflection.ParseObject(t, r.DeviceToken), (r, t) => r.SessionKey = Reflection.ParseObject(t, r.SessionKey), (r, t) => r.UpdateTime = Reflection.ParseObject(t, r.UpdateTime), (r, t) => r.user_id = Reflection.ParseObject(t, r.user_id) };
        readonly static string[] _Columns = new string[] { "ClientIP", "Content", "ContentType", "DeviceToken", "SessionKey", "UpdateTime", "user_id" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "ClientIP", this.ClientIP);
            AppendValue(action, "Content", this.Content);
            AppendValue(action, "ContentType", this.ContentType);
            AppendValue(action, "DeviceToken", this.DeviceToken);
            AppendValue(action, "SessionKey", this.SessionKey);
            AppendValue(action, "UpdateTime", this.UpdateTime);
            AppendValue(action, "user_id", this.user_id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[7];
            cols[0] = RecordColumn.Column("ClientIP", this.ClientIP);
            cols[1] = RecordColumn.Column("Content", this.Content);
            cols[2] = RecordColumn.Column("ContentType", this.ContentType);
            cols[3] = RecordColumn.Column("DeviceToken", this.DeviceToken);
            cols[4] = RecordColumn.Column("SessionKey", this.SessionKey);
            cols[5] = RecordColumn.Column("UpdateTime", this.UpdateTime);
            cols[6] = RecordColumn.Column("user_id", this.user_id);
            return cols;
        }

    }
}

