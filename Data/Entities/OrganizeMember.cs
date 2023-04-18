using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    public partial class OrganizeMember : Record
    {
        public int? org_id { get; set; }
        public Guid? user_id { get; set; }
        public int? MemberType { get; set; }
        public int? Seq { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
