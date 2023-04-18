using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{

    /// <summary>
    /// 
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// 国家
        /// </summary>
        Nation = 1,
        /// <summary>
        /// 省份
        /// </summary>
        Province = 2,
        /// <summary>
        /// 城市
        /// </summary>
        City = 3,
        /// <summary>
        /// 县区
        /// </summary>
        Region = 4
    }
    public class Location : Record
    {
        public int? Id
        {
            get;
            set;
        }
        public LocationType? Type
        {
            get;
            set;
        }
        public string ZipCode
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public int? ParentId
        {
            get;
            set;
        }


    }
    public partial class Menu : Record
    {
        public int? Id
        {
            get; set;
        }
        public String Icon
        {
            get; set;
        }
        public String Caption
        {
            get; set;
        }
        public string Url
        {
            get; set;
        }
        public int? Seq
        {
            get; set;
        }
        public int? ParentId
        {
            get; set;
        }

        public bool? IsHidden
        {
            get; set;
        }
        /// <summary>
        /// 所属站点默认为0
        /// </summary>
        public int? Site
        {
            get; set;
        }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public int? Type
        {
            get; set;
        }
        /// <summary>
        /// 参数
        /// </summary>
        public string Value { get; set; }

    }



}
