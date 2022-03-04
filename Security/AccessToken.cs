using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UMC.Data;

namespace UMC.Security
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public class AccessToken
    {

        private AccessToken()
        {
            this.Data = new Hashtable();
        }
        /// <summary>
        /// 过期时间，单位为秒，0为不过期
        /// </summary>
        public int Timeout
        {
            get;
            private set;
        }
        public string Username
        {
            get;
            private set;
        }
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid? UId
        {
            get;
            private set;
        }
        /// <summary>
        /// 关联的ID
        /// </summary>
        public Guid? Id
        {
            get;
           internal set;
        }
        /// <summary>
        /// 角色
        /// </summary>
        public string Roles
        {
            get;
            private set;
        }
        /// <summary>
        /// 最后一次活动时间
        /// </summary>
        public int? ActiveTime
        {
            get;
            private set;
        }
        [UMC.Data.JSON]
        public Hashtable Data
        {
            get;
            private set;
        }
        /// <summary>
        /// 退出
        /// </summary>
        public void SignOut(String clientIp)
        {
            Login(UMC.Security.Identity.Create(this.Id.Value, "?", String.Empty), null, clientIp);
        }
        public AccessToken Put(string key, string value)
        {
            if (String.IsNullOrEmpty(key) == false)
            {
                if (String.IsNullOrEmpty(value))
                {
                    this.Data.Remove(key);
                }
                else
                {
                    this.Data[key] = value;
                }
            }
            return this;
        }
        /// <summary>
        /// 创建登录令牌
        /// </summary>
        /// <param name="user">身份</param>
        /// <param name="deviceId">令牌ID</param>
        /// <param name="deviceType">设备类型</param>
        /// <param name="timeout">过期时间</param>
        /// <returns></returns>
        public static AccessToken Create(Identity user, Guid deviceId, int timeout)
        {
            var auth = new AccessToken();
            //auth.ContentType = deviceType;
            auth.Timeout = timeout;
            auth.Id = deviceId;
            auth.Username = user.Name;
            auth.UId = user.Id;
            auth.ActiveTime = UMC.Data.Utility.TimeSpan();
            auth.Roles = null;

            switch (user.Name)
            {
                case "#":
                case "?":
                    if (String.IsNullOrEmpty(user.Alias) == false)
                    {
                        auth.Data["#"] = user.Alias;
                    }
                    break;
                default:

                    auth.Data["#"] = user.Alias;
                    if (user.Roles != null)
                    {
                        auth.Roles = String.Join(",", user.Roles);
                    }
                    break;
            }
            return auth;
        }
        public AccessToken Put(System.Collections.Specialized.NameValueCollection NameValue)
        {
            for (var i = 0; i < NameValue.Count; i++)
            {
                var key = NameValue.GetKey(i);
                if (String.IsNullOrEmpty(key) == false)
                {
                    var value = NameValue.Get(i);
                    if (String.IsNullOrEmpty(value))
                    {
                        this.Data.Remove(key);
                    }
                    else
                    {
                        this.Data[key] = value;
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// 提交修改访问票据
        /// </summary>
        public void Commit(String clientIP)
        {
            this.Commit(null, clientIP);
        }
        public void Commit(string deviceType, String clientIP)
        {
            this.ActiveTime = UMC.Data.Utility.TimeSpan();///

            if (this.UId.HasValue)
            {
                var sesion = new Session<UMC.Security.AccessToken>(this, this.Id.ToString());
                sesion.Commit(this.UId.Value, deviceType, false, clientIP);
            }
        }
        public UMC.Security.Identity Identity()
        {

            var Alias = this.Data["#"] as string ?? String.Empty;
            int cuttime = UMC.Data.Utility.TimeSpan();
            if (this.Timeout > 0 && ((this.ActiveTime ?? 0) + this.Timeout) <= cuttime)
            {
                this.UId = this.Id;
                return UMC.Security.Identity.Create(this.Id.Value, "?", Alias);
            }
            if (String.IsNullOrEmpty(this.Username))
            {
                this.UId = this.Id;

                return UMC.Security.Identity.Create(this.Id.Value, "?", Alias);
            }
            if (this.UId == this.Id)
            {
                return UMC.Security.Identity.Create(this.UId ?? this.Id.Value, "?", Alias);
            }
            switch (this.Username)
            {
                case "?":
                    return UMC.Security.Identity.Create(this.UId ?? this.Id.Value, "?", Alias);
                case "#":
                    if (this.UId.HasValue)
                    {
                        return UMC.Security.Identity.Create(this.UId.Value, "#", Alias);
                    }
                    else
                    {
                        return UMC.Security.Identity.Create(this.Id.Value, "?", Alias);
                    }
                default:
                    if (this.UId.HasValue)
                    {
                        if (String.IsNullOrEmpty(this.Roles))
                        {
                            return UMC.Security.Identity.Create(this.UId.Value, this.Username, Alias);

                        }
                        else
                        {
                            return UMC.Security.Identity.Create(this.UId.Value, this.Username
                                , Alias, this.Roles.Split(new String[] { "," }, StringSplitOptions.None));
                        }
                    }
                    else
                    {
                        return UMC.Security.Identity.Create(this.Id.Value, "?", Alias);
                    }
            }
        }
        /// <summary>
        /// 登录，默认30分钟过期
        /// </summary>
        /// <param name="user"></param>
        /// <param name="deviceId">设备令牌ID</param>
        /// <returns></returns>
        public void Login(Identity user, string deviceType, String clientIp)
        {
            Login(user, 30 * 60, deviceType, false, clientIp);

        }
        /// <summary>
        /// 登录如果unqiue为false，则timeout为30分钟
        /// </summary>
        /// <param name="Id">身份</param>
        /// <param name="deviceId">设备Id</param>
        /// <param name="deviceType">设备类型</param>
        /// <param name="unqiue">记录登录</param>
        /// <returns></returns>
        public void Login(Identity user, string deviceType, bool unqiue, String clientIp)
        {
            Login(user, unqiue ? 0 : 30 * 60, deviceType, unqiue, clientIp);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">身份</param>
        /// <param name="deviceId">设备Id</param>
        /// <param name="timeout">过期时间</param>
        /// <param name="deviceType">设置类型</param>
        /// <param name="unqiue">记录登录</param>
        /// <returns></returns>
        public void Login(Identity user, int timeout, string deviceType, bool unqiue, String clientIp)
        {
            var auth = this;
            //auth.ContentType = deviceType;
            auth.Timeout = timeout;
            auth.Username = user.Name;
            auth.UId = user.Id;
            auth.ActiveTime = UMC.Data.Utility.TimeSpan();
            auth.Roles = null;

            switch (user.Name)
            {
                case "#":
                case "?":
                    if (String.IsNullOrEmpty(user.Alias) == false)
                    {
                        auth.Data["#"] = user.Alias;
                    }
                    break;
                default:

                    auth.Data["#"] = user.Alias;
                    if (user.Roles != null)
                    {
                        auth.Roles = String.Join(",", user.Roles);
                    }
                    break;
            }

            var sesion = new Session<UMC.Security.AccessToken>(this, this.Id.ToString());
            sesion.Commit(this.UId.Value, deviceType, unqiue, clientIp);

        }

        public string Get(string key)
        {

            return this.Data[key] as string;

        }
        public bool IsInRole(string role)
        {
            if (String.IsNullOrEmpty(this.Roles))
            {
                return false;
            }
            var roles = $",{this.Roles},";

            if (roles.Contains($",{Membership.AdminRole},"))
            {
                return true;
            }
            else if (roles.Contains($",{role},"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
