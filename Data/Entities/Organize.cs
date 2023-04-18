using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    public partial class Organize : Record
    {
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public String Caption { get; set; }
        public String Identifier { get; set; }
        public int? Seq { get; set; }
        public DateTime? ModifyTime { get; set; }
        public int? Members { get; set; }


    }
}
