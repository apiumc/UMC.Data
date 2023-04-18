using System;
namespace UMC.Data.Entities
{
    public partial class Password : Record
    {
        public Guid? Key
        {
            get;set;
        }
        public byte[] Body
        {
            get; set;
        }
    }
}
