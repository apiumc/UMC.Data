using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UMC.Data;
using UMC.Data.Entities;

namespace UMC.Security
{

    /// <summary>
    /// 用户安全标签
    /// </summary>
    public enum UserFlags
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 锁定
        /// </summary>
        Lock = 1,
        /// <summary>
        /// 要更新密码
        /// </summary>
        Changing = 2,
        /// <summary>
        /// 不能更新密码
        /// </summary>
        UnChangePasswork = 4,
        /// <summary>
        /// 没有通过验证
        /// </summary>
        UnVerification = 8,
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 16

    }
    /// <summary> 
    /// 用户管理类
    /// </summary>
    public class Membership
    {
        public const string SessionCookieName = "device";

        /// <summary>
        /// 管理员角色
        /// </summary>
        public const String AdminRole = "Administrators";
        /// <summary>
        /// 一般用户角色
        /// </summary>
        public const String UserRole = "Users";
        /// <summary>
        /// 来宾角色
        /// </summary>
        public const String GuestRole = "Guest";
        /// <summary>
        /// 实例
        /// </summary>
        /// <returns></returns>
        public static Membership Instance()
        {
            return _Instance;
        }
        static Membership _Instance = new Membership();
        public static void Instance(Membership membership)
        {
            _Instance = membership;
        }
        /// <summary>
        /// 获取用户身份
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        //public abstract List<Identity> Identity(params string[] names);

        /// <summary>
        /// 获取用户身份
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        //public abstract Identity Identity(string username);
        /// <summary> 
        /// 检验用户密码
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="maxtimes">失败最大次数</param>
        /// <returns>失败次数</returns>
        //public abstract int Password(string username, string password, int maxtimes);

        /// <summary>
        /// 获取用户身价
        /// </summary>
        /// <param name="sessionKey">终端标示</param>
        /// <param name="contentType">终端类型</param>
        /// <returns></returns>
        //public virtual UMC.Security.Principal Authorization(string sessionKey, String contentType, string clientip)
        //{


        //}

        public virtual int Password(string username, string password, int max)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            var user = Data.DataFactory.Instance().User(username);
            if (user == null)
            {
                return -1;
            }
            else
            {
                if (((user.Flags ?? Security.UserFlags.Lock) & Security.UserFlags.Lock) == Security.UserFlags.Lock)
                {
                    return -2;
                }
                else if (((user.Flags ?? Security.UserFlags.Disabled) & Security.UserFlags.Disabled) == Security.UserFlags.Disabled)
                {
                    return -3;
                }


                var destPwd = Data.DataFactory.Instance().Password(user.Id.Value);
                // var passwrod = Data.DataFactory.Instance().Password(username);

                if (String.IsNullOrEmpty(destPwd))
                {
                    return 0;
                }
                // UMC.Data.Utility.DES(Convert.FromBase64String(passwrod), user.Id.Value);
                StringComparison comparison = StringComparison.CurrentCulture;
                var spIndex = password.IndexOf(':');
                if (spIndex > 0)
                {
                    var md5pwd = password.Substring(spIndex + 1);
                    var sp = password.Substring(0, spIndex);
                    if (md5pwd.Length == 32)
                    {
                        password = password.Substring(spIndex + 1);
                        destPwd = UMC.Data.Utility.MD5(sp + destPwd);
                        comparison = StringComparison.CurrentCultureIgnoreCase;
                    }
                }
                if (String.Equals(destPwd, password, comparison))
                {
                    Data.DataFactory.Instance().Put(new User { Username = username, VerifyTimes = 0, ActiveTime = DateTime.Now });
                    return 0;
                }
                else if (max > 0)
                {
                    var s = new User { VerifyTimes = (user.VerifyTimes ?? 0) + 1, Username = username };
                    if (max <= user.VerifyTimes)
                    {
                        s.Flags = (user.Flags ?? Security.UserFlags.Normal) | Security.UserFlags.Lock;
                    }
                    Data.DataFactory.Instance().Put(s);
                    return s.VerifyTimes ?? 0;
                }
                else
                {
                    return 1;
                }
            }
        }
        public virtual UMC.Security.Identity Identity(string name, int accountType)
        {
            var acount = Data.DataFactory.Instance().Account(name, accountType);

            if (acount == null)
            {
                return null;
            }

            var user = this.Identity(acount.user_id.Value);
            if (user == null)
            {
                var a = UMC.Security.Account.Create(acount);
                return UMC.Security.Identity.Create(acount.user_id.Value, "#", (a.Items["Alias"] as string) ?? " 关联用户");
            }
            return user;
        }

        public virtual bool Password(string Username, string password)
        {
            var user = Data.DataFactory.Instance().User(Username);
            if (user != null)
            {
                Data.DataFactory.Instance().Password(user.Id.Value, password);

                return true;
            }
            return false;

        }


        public virtual Guid CreateUser(string username, string alias)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            if (Data.DataFactory.Instance().User(username) == null)
            {
                var sn = UMC.Data.Utility.Guid(username, true).Value;

                Data.DataFactory.Instance().Put(new User
                {
                    Alias = alias,
                    Flags = UMC.Security.UserFlags.Normal,
                    Id = sn,
                    RegistrTime = DateTime.Now,
                    //OrganizeId = OrganizeId,
                    Username = username
                });
                //if (String.IsNullOrEmpty(password) == false)
                //{
                //    Data.DataFactory.Instance().Password(sn, password);
                //}
                return sn;
            }
            return Guid.Empty;
        }
        public virtual bool ChangeFlags(string username, UMC.Security.UserFlags flags)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");


            Data.DataFactory.Instance().Put(new User { Username = username, Flags = flags });
            return true;
        }


        public virtual UMC.Security.Identity Identity(string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            var user = Data.DataFactory.Instance().User(username);
            if (user == null)
            {
                return null;
            }
            var flags = user.Flags ?? Security.UserFlags.Normal;
            if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock)
            {
                return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias);
            }
            var roles = new List<string>();



            UMC.Data.Utility.Each(Data.DataFactory.Instance().Roles(user.Id.Value), dr =>
            {
                roles.Add(dr.Rolename);
            });

            return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias, roles.ToArray());

        }



        public virtual bool AddRole(string Username, params string[] roles)
        {
            if (String.IsNullOrEmpty(Username)) throw new ArgumentNullException("username");
            if (roles.Length > 0)
            {
                var user = Data.DataFactory.Instance().User(Username);
                if (user != null)
                {
                    var rols = Data.DataFactory.Instance().Roles(user.Id.Value).ToList();
                    var addRs = roles.Where(d => !rols.Exists(c => String.Equals(c.Rolename, d, StringComparison.CurrentCultureIgnoreCase)));//.ToList();
                    if (addRs.Count() > 0)
                    {
                        var allRoles = Data.DataFactory.Instance().Roles();
                        if (allRoles.Length == 0)
                        {
                            allRoles = new Data.Entities.Role[]{new  Data.Entities.Role {  Rolename=AdminRole},
                                new  Data.Entities.Role {  Rolename= UserRole},new  Data.Entities.Role {  Rolename=GuestRole} };
                        }
                        var rs = allRoles.Where(a => addRs.Contains(a.Rolename)).Union(rols).ToArray();
                        Data.DataFactory.Instance().Put(user.Id.Value, rs);
                    }

                }
            }

            return false;
        }

        public virtual bool ChangeAlias(string username, string alias)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            return Data.DataFactory.Instance().Put(new User { Username = username, Alias = alias });
        }



        public virtual Security.Identity Identity(Guid id)
        {

            var user = Data.DataFactory.Instance().User(id);
            if (user == null)
            {
                return null;
            }
            var flags = user.Flags ?? Security.UserFlags.Normal;
            if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock)
            {
                return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias);
            }
            var roles = new List<string>();



            UMC.Data.Utility.Each(Data.DataFactory.Instance().Roles(user.Id.Value), dr => roles.Add(dr.Rolename));

            return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias, roles.ToArray());


        }
        public virtual Security.Identity CreateUser(Guid id, string username, string alias)
        {

            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");


            if (Data.DataFactory.Instance().User(username) == null && Data.DataFactory.Instance().User(id) == null)
            {
                Data.DataFactory.Instance().Put(new User
                {
                    Alias = alias,
                    Flags = UMC.Security.UserFlags.Normal,
                    Id = id,
                    RegistrTime = DateTime.Now,
                    //OrganizeId = OrganizeId,
                    Username = username
                });


                return UMC.Security.Identity.Create(id, username, alias);
            }

            return null;

        }

        public virtual List<Identity> Identity(params string[] names)
        {
            var ids = new List<Identity>();
            if (names.Length > 0)
            {
                var ns = new List<String>(names);

                UMC.Data.Utility.Each(Data.DataFactory.Instance().Users(names), dr =>
                {
                    ns.RemoveAll(n => String.Equals(n, dr.Username, StringComparison.CurrentCultureIgnoreCase));
                    ids.Add(Security.Identity.Create(dr.Id.Value, dr.Username, dr.Alias));
                });
                foreach (var n in names)
                {
                    if (n.StartsWith("~"))
                    {
                        ids.Add(Security.Identity.Create(n, "自动流程"));
                    }
                    else
                    {

                        ids.Add(Security.Identity.Create(n, n));
                    }
                }

            }
            return ids;
        }



    }

}
