using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{
    /// <summary>
    /// 相册
    /// </summary>
    public class Picture
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
        public string Caption
        {
            get;
            set;
        }
        public string Location
        {
            get;
            set;
        }

        public DateTime? UploadDate
        {
            get;
            set;
        }
    }
}
