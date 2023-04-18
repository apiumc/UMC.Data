using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    /// <summary>
    /// 授权符
    /// </summary>
    public partial class Authority : Record
    {
        public int? Site
        {
            get; set;
        }

        public string Key
        {
            get;
            set;
        }
        public string Desc
        {
            get;
            set;
        }
        public byte[] Body
        {
            get;
            set;
        }

    }
}
