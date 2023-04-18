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
        /// 没有通过验证
        /// </summary>
        UnVerification = 8

    }
    /// <summary> 
    /// 用户管理类
    /// </summary>
    public class Membership
    {

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
                var flags = user.Flags ?? Security.UserFlags.Normal;
                if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock)
                {
                    return -2;
                }
                else if (user.IsDisabled == true)
                {
                    return -3;
                }


                var destPwd = Data.DataFactory.Instance().Password(user.Id.Value);

                if (String.IsNullOrEmpty(destPwd))
                {
                    return -4;
                }
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
                        s.Flags = flags | Security.UserFlags.Lock;
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
            var acount = Data.DataFactory.Instance().Account(name);

            if (acount == null)
            {
                return null;
            }

            var user = this.Identity(acount.user_id.Value);
            if (user == null)
            {
                var a = UMC.Security.Account.Create(acount);
                return UMC.Security.Identity.Create(acount.user_id.Value, "#", (a.Items["Alias"] as string) ?? "关联用户");
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
                var sn = UMC.Data.Utility.NewGuid();// username, true).Value;

                Data.DataFactory.Instance().Put(new User
                {
                    Alias = alias,
                    Flags = UMC.Security.UserFlags.Normal,
                    Id = sn,
                    RegistrTime = DateTime.Now,
                    Username = username
                });
                using (System.IO.Stream stream = typeof(Membership).Assembly//UMC.Proxy
                                        .GetManifestResourceStream("UMC.Data.Resources.header.png"))
                {
                    WebResource.Instance().Transfer(stream, $"images/{sn}/1/0.png");
                }
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


        public virtual UMC.Security.Identity Identity(int site, string username)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            var user = Data.DataFactory.Instance().User(username);
            if (user == null)
            {
                return null;
            }
            var flags = user.Flags ?? Security.UserFlags.Normal;
            if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock || (user.IsDisabled ?? false) == true)
            {
                return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias);
            }
            var orgs = new List<String>();
            Utility.Each(Data.DataFactory.Instance().OrganizesTree(user), r =>
            {
                orgs.Add(r.ToString());
            });
            return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias, Data.DataFactory.Instance().Roles(user.Id.Value, site), orgs.ToArray());

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
            if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock || (user.IsDisabled ?? false) == true)
            {
                return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias);
            }
            var orgs = new List<String>();
            Utility.Each(Data.DataFactory.Instance().OrganizesTree(user), r =>
            {
                orgs.Add(r.ToString());
            });
            return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias, Data.DataFactory.Instance().Roles(user.Id.Value, 0), orgs.ToArray());

        }



        public virtual bool AddRole(string Username, int site, params string[] roles)
        {
            if (String.IsNullOrEmpty(Username)) throw new ArgumentNullException("username");
            if (roles.Length > 0)
            {
                var user = Data.DataFactory.Instance().User(Username);
                if (user != null)
                {
                    foreach (var k in roles)
                    {
                        Data.DataFactory.Instance().Put(new UserToRole
                        {
                            Rolename = k,
                            Site = site,
                            user_id = user.Id.Value
                        });
                    }


                }
            }

            return false;
        }

        public virtual void ChangeAlias(string username, string alias)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            Data.DataFactory.Instance().Put(new User { Username = username, Alias = alias });
        }



        public virtual Security.Identity Identity(Guid id)
        {

            var user = Data.DataFactory.Instance().User(id);
            if (user == null)
            {
                return null;
            }
            var flags = user.Flags ?? Security.UserFlags.Normal;
            if ((flags & Security.UserFlags.Lock) == Security.UserFlags.Lock || (user.IsDisabled ?? false) == true)
            {
                return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias);
            }
            var orgs = new List<String>();
            Utility.Each(Data.DataFactory.Instance().OrganizesTree(user), r =>
            {
                orgs.Add(r.ToString());
            });
            return UMC.Security.Identity.Create(user.Id.Value, user.Username, user.Alias, Data.DataFactory.Instance().Roles(user.Id.Value, 0), orgs.ToArray());


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
