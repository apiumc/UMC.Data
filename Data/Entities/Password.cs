using System;
namespace UMC.Data.Entities
{
    public class Password
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
