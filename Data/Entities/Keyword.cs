﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{


    /// <summary>
    /// 搜索关键字
    /// </summary>
    public partial class Keyword : Record
    {
        public string Key
        {
            get; set;
        }
        public Guid? user_id
        {
            get; set;
        }
        public int? Time
        {
            get; set;
        }
    }
}
