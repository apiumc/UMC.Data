using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    public class Log
    {
        public string Key { get; set; }
        public string Username { get; set; }
        public int? Time { get; set; }
        public string Path { get; set; }
        public string IP { get; set; }
        public string UserAgent { get; set; }

        public string Referrer { get; set; }

        public int? Quantity { get; set; }

        public int? Duration
        {
            get; set;
        }
        public int? Status
        {
            get; set;
        }
        public string Context { get; set; }

    }
}
