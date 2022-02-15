using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    /// <summary>
    /// 编号
    /// </summary>
    public class Number
    {
        public string CodeKey
        {
            get;
            set;
        }
        public string Type
        {
            get;
            set;
        }
        public string Format
        {
            get;
            set;
        }
        public int? Value
        {
            get;
            set;
        }
        public DateTime? UpdateDate
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
    }
    public class Session
    {
        public string SessionKey
        {
            get;
            set;
        }
        public Guid? user_id
        {
            get;
            set;
        }
        public string Content
        {
            get;
            set;
        }
        public string ContentType
        {
            get;
            set;
        }
        public DateTime? UpdateTime
        {
            get;
            set;
        }
        public string DeviceToken { get; set; }
    }
    public class Cache
    {
        public Guid? Id
        {
            get;
            set;
        }
        public string CacheKey
        {
            get;
            set;
        }
        public DateTime? BuildDate
        {
            get;
            set;
        }
        public DateTime? ExpiredTime
        {
            get;
            set;
        }
       // [JSON]
        public String CacheData
        {
            get;
            set;
        }

    }
}
