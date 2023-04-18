using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    public partial class Config : Record
    {
        public string ConfKey
        {
            get;
            set;
        }
        public string ConfValue
        {
            get;
            set;
        }
    }
}
