using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    /// <summary>
    /// 相册
    /// </summary>
    public partial class Picture : Record
    {
        public Guid? group_id
        {
            get;
            set;
        }
        public byte[] Value
        {
            get;
            set;
        }
        public Guid? user_id
        {
            get;
            set;
        }

        public int? UploadTime
        {
            get;
            set;
        }
    }
}
