using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Entities
{

    /// <summary>
    /// 基础用户
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid? Id
        {
            get;
            set;
        }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username
        {
            get;
            set;
        }
        /// <summary>
        /// 别名
        /// </summary>
        public string Alias
        {
            get;
            set;
        }
        /// <summary>
        /// 用户标志
        /// </summary>
        public UMC.Security.UserFlags? Flags
        {
            get;
            set;
        }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegistrTime
        {
            get;
            set;
        }
        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime? ActiveTime
        {
            get;
            set;
        }
        /// <summary>
        /// 密码验证失败次数
        /// </summary>
        public int? VerifyTimes
        {
            get;
            set;
        }
        /// <summary>
        /// 个性签名
        /// </summary>
        public string Signature
        {
            get; set;
        }
        
    }
    public class Role
    {
        //public Guid? Id
        //{
        //    get;
        //    set;
        //}
        public string Rolename
        {
            get;
            set;
        }
        public string Explain
        {
            get;
            set;
        }
    }
    public class UserToRole
    {
        public String Rolename
        {
            get;
            set;
        }
        public Guid? user_id
        {
            get;
            set;
        }
    }
    /// <summary>
    /// 账户管理
    /// </summary>
    public class Account
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public Guid? user_id
        {
            get;
            set;
        }
        /// <summary>
        /// 账户名
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 账户类型
        /// </summary>
        public int? Type
        {
            get;
            set;
        }
        /// <summary>
        /// 标志
        /// </summary>
        public UMC.Security.UserFlags? Flags
        {
            get;
            set;
        }

        /// <summary>
        /// 来源
        /// </summary>
        public string ForId
        {
            get;
            set;
        }

        public string ConfigData
        {
            get;
            set;
        }

    }
}
