using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    public partial class Session : Record
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

        public string ClientIP
        {
            get; set;
        }
    }
    public partial class Cache : Record
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
        public String CacheData
        {
            get;
            set;
        }

    }
}
