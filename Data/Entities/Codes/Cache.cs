using System;
using System.Collections.Generic;
using UMC.Data;
namespace UMC.Data.Entities
{
    public partial class Cache
    {
        readonly static Action<Cache, object>[] _SetValues = new Action<Cache, object>[] { (r, t) => r.BuildDate = Reflection.ParseObject(t, r.BuildDate), (r, t) => r.CacheData = Reflection.ParseObject(t, r.CacheData), (r, t) => r.CacheKey = Reflection.ParseObject(t, r.CacheKey), (r, t) => r.ExpiredTime = Reflection.ParseObject(t, r.ExpiredTime), (r, t) => r.Id = Reflection.ParseObject(t, r.Id) };
        readonly static string[] _Columns = new string[] { "BuildDate", "CacheData", "CacheKey", "ExpiredTime", "Id" };
        protected override void SetValue(string name, object obv)
        {
            var index = Utility.Search(_Columns, name, StringComparer.CurrentCultureIgnoreCase);
            if (index > -1) _SetValues[index](this, obv);
        }
        protected override void GetValues(Action<String, object> action)
        {
            AppendValue(action, "BuildDate", this.BuildDate);
            AppendValue(action, "CacheData", this.CacheData);
            AppendValue(action, "CacheKey", this.CacheKey);
            AppendValue(action, "ExpiredTime", this.ExpiredTime);
            AppendValue(action, "Id", this.Id);
        }

        protected override RecordColumn[] GetColumns()
        {
            var cols = new RecordColumn[5];
            cols[0] = RecordColumn.Column("BuildDate", this.BuildDate);
            cols[1] = RecordColumn.Column("CacheData", this.CacheData);
            cols[2] = RecordColumn.Column("CacheKey", this.CacheKey);
            cols[3] = RecordColumn.Column("ExpiredTime", this.ExpiredTime);
            cols[4] = RecordColumn.Column("Id", this.Id);
            return cols;
        }

    }
}

