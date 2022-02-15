using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    public class Organize
    {
        public Guid? Id { get; set; }
        public Guid? ParentId { get; set; }
        public String Caption { get; set; }
        public String Identifier { get; set; }
        public int? Seq { get; set; }
        public DateTime? ModifyTime { get; set; }

        public int? Members { get; set; }

       // public bool? IsHide { get; set; }
    }
    public class OrganizeMember
    {
        public Guid? org_id { get; set; }
        public Guid? user_id { get; set; }
        public int? MemberType { get; set; }
        public int? Seq { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
