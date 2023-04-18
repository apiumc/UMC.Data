using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMC.Data.Entities;

namespace UMC.Data
{

    public class DataFactory
    {
        static DataFactory()
        {
            UMC.Data.Reflection.Instance(new UMC.Security.Reflection(), UMC.Data.Reflection.Instance().Provider);
            HotCache.Register<Account>("Name").Register("user_id", "Type");
            HotCache.Register<Cache>("Id", "CacheKey");
            HotCache.Register<Session>("SessionKey").Register("user_id", "SessionKey");

            HotCache.Register<User>("Id").Register("Username");
            HotCache.Register<Password>("Key");
            HotCache.Register<Organize>("Id").Register("ParentId", "Id");


            HotCache.Register<OrganizeMember>("org_id", "user_id").Register("user_id", "org_id");
            HotCache.Register<Authority>("Site", "Key");
            HotCache.Register<Picture>("group_id");

            HotCache.Register<UserToRole>("Site", "user_id", "Rolename").Register("Site", "Rolename", "user_id");
            HotCache.Register<Role>("Site", "Rolename");
            HotCache.Register<Config>("ConfKey");

            HotCache.Register<Menu>("Site", "Id").Register("Site", "ParentId", "Id");
            HotCache.Register<Keyword>("user_id", "Key").Register("user_id", "Time", "Key");



        }
        static DataFactory _Instance = new DataFactory();
        public static void Instance(DataFactory dataFactory)
        {
            _Instance = dataFactory;
        }
        public static DataFactory Instance()
        {
            return _Instance;
        }
        public virtual void Delete(Session session)
        {
            HotCache.Cache<Session>().Delete(session);

        }

        public virtual void Delete(UserToRole session)
        {
            HotCache.Cache<UserToRole>().Delete(session);

        }
        public virtual void Put(Session session)
        {

            HotCache.Cache<Session>().Put(session);

        }
        public virtual Session Session(string sessionKey)
        {
            return HotCache.Cache<Session>().Get(new Data.Entities.Session { SessionKey = sessionKey });
        }
        public virtual Session[] Session(Guid user_id)
        {
            int index = 0;
            return HotCache.Cache<Session>().Find(new Session
            {
                user_id = user_id,
            }, index, out index);

        }
        public virtual Config Config(string key)
        {
            return HotCache.Get(new Config { ConfKey = key });
        }
        public virtual void Put(Config cache)
        {
            HotCache.Put(cache);
        }
        public virtual String Password(Guid key)
        {
            var password = HotCache.Get(new Password { Key = key });

            if (password != null && password.Body != null)
            {
                return UMC.Data.Utility.DES(password.Body, password.Key.Value);
            }
            return null;

        }
        public virtual void Delete(Config config)
        {
            HotCache.Delete(config);
        }
        public virtual void Delete(Password password)
        {
            if (password.Key.HasValue)
            {
                HotCache.Delete(new Password
                {
                    Key = password.Key
                });
            }

        }
        public virtual void Password(Guid key, String pwd)
        {
            HotCache.Cache<Password>().Put(new Password
            {
                Key = key,
                Body = UMC.Data.Utility.DES(pwd, key)
            });

        }
        public virtual void Delete(Cache cache)
        {
            HotCache.Cache<Cache>().Delete(new Data.Entities.Cache
            {
                Id = cache.Id.Value,
                CacheKey = cache.CacheKey ?? String.Empty
            });
        }
        public virtual void Delete(Organize organize)
        {
            HotCache.Cache<Organize>().Delete(new Data.Entities.Organize
            {
                Id = organize.Id.Value
            });
        }
        public virtual void Delete(OrganizeMember organizeMember)
        {
            HotCache.Cache<OrganizeMember>().Delete(organizeMember);
        }

        public virtual void Delete(Picture picture)
        {

            HotCache.Cache<Picture>().Delete(picture);
        }
        public virtual void Put(Keyword keyword)
        {
            if (String.IsNullOrEmpty(keyword.Key) == false && keyword.user_id.HasValue)
            {
                keyword.Time = Utility.TimeSpan();
                var hot = HotCache.Cache<Keyword>();
                hot.Put(keyword);
                var keyh = hot.Get(new Entities.Keyword { Key = keyword.Key, user_id = Guid.Empty });

                if (keyh == null)
                {
                    hot.Put(new Entities.Keyword { Key = keyword.Key, user_id = Guid.Empty, Time = 1 });
                }
                else
                {
                    hot.Put(new Entities.Keyword { Key = keyword.Key, user_id = Guid.Empty, Time = (keyh.Time ?? 0) + 1 });
                }
            }
        }
        public virtual Keyword[] Keyword(Guid userId)
        {
            var hot = HotCache.Cache<Keyword>();
            int index;
            return hot.Find(new Keyword { user_id = userId }, true, 0, 20, out index);
        }

        public virtual Cache Cache(Guid id, string cacheKey)
        {
            return HotCache.Cache<Cache>().Get(new Data.Entities.Cache
            {
                Id = id,
                CacheKey = cacheKey ?? String.Empty
            });
        }


        public virtual Picture[] Pictures(params Guid[] gid)
        {
            int index;
            return HotCache.Cache<Picture>().Find(new Data.Entities.Picture(), 0, 500, out index, "group_id", gid);
        }

        public virtual Picture Picture(Guid guid)
        {
            return HotCache.Cache<Picture>().Get(new Data.Entities.Picture
            {
                group_id = guid
            });

        }
        public virtual void Put(Cache cache)
        {
            HotCache.Cache<Cache>().Put(cache);

        }

        public virtual void Put(Picture picture)
        {
            HotCache.Cache<Picture>().Put(picture);

        }
        public virtual Authority[] Search(Authority search, int start, int limit, out int nextIndex)
        {
            return HotCache.Cache<Authority>().Find(search, start, limit, out nextIndex);

        }
        public virtual User[] Search(User search, int start, int limit, out int nextIndex)
        {
            var seacher = HotCache.Cache<User>().Search<User>();

            seacher.And().Equal(new Entities.User { IsDisabled = search.IsDisabled, Flags = search.Flags });
            seacher.Or().Like(new UMC.Data.Entities.User
            {
                Username = search.Username,
                Alias = search.Alias
            });

            return seacher.Query(new User(), start, limit, out nextIndex);
        }
        public virtual void Put(User user)
        {
            if (user.Id.HasValue)
            {
                HotCache.Cache<User>().Put(user);
            }
            else if (String.IsNullOrEmpty(user.Username) == false)
            {
                var u = HotCache.Cache<User>().Get(new Entities.User { Username = user.Username });
                if (u != null)
                {
                    user.Id = u.Id;
                    HotCache.Cache<User>().Put(user);
                }
            }
        }

        public virtual void Put(Role role)
        {
            if (role.Site.HasValue == false)
            {
                role.Site = 0;

            }
            HotCache.Cache<Role>().Put(role);

        }

        public virtual void Put(UserToRole userToRole)
        {
            HotCache.Cache<UserToRole>().Put(userToRole);

        }

        public virtual Role Role(int site, string rolename)
        {
            return HotCache.Cache<Role>().Get(new Data.Entities.Role
            {
                Site = site,
                Rolename = rolename
            });

        }
        public virtual Role[] Roles(int site)
        {
            int index;
            return HotCache.Cache<Role>().Find(new Role { Site = site }, 0, out index);
        }


        public virtual string[] Roles(Guid userid, int site)
        {

            var ut = HotCache.Cache<UserToRole>();
            int index;
            var rows = ut.Find(new UserToRole { user_id = userid, Site = site }, 0, out index);

            var rls = new List<String>();
            foreach (var r in rows)
            {

                rls.Add(r.Rolename);
            };

            return rls.ToArray();
        }

        public virtual User User(string username)
        {
            return HotCache.Cache<User>().Get(new Data.Entities.User
            {
                Username = username
            });
        }

        public virtual User User(Guid userId)
        {
            return HotCache.Cache<User>().Get(new Data.Entities.User
            {
                Id = userId
            });

        }
        public virtual User[] Users(int site, string roleName)
        {
            int next;
            var rols = HotCache.Cache<UserToRole>().Find(new UserToRole { Site = site, Rolename = roleName }, 0, out next);
            var ks = new List<String>();
            foreach (var k in rols)
            {
                ks.Add(k.Rolename);

            }
            return Users(ks.ToArray());
        }

        public virtual User[] Users(params Guid[] userIds)
        {
            return HotCache.Cache<User>().Find(new User { }, "Id", userIds);
        }
        public virtual User[] Users(Data.Entities.Organize organize)
        {
            int index = 0;
            var ut = HotCache.Cache<OrganizeMember>().Find(new OrganizeMember { org_id = organize.Id.Value }, 0, out index)
                .OrderBy(r => r.Seq ?? 0);
            var ids = new List<Guid>();

            foreach (var k in ut)
            {
                ids.Add(k.user_id.Value);

            }
            if (ids.Count > 0)
            {
                return Users(ids.ToArray()).OrderBy(r => ids.FindIndex(id => id == r.Id.Value)).ToArray();
            }
            return new User[0];

        }
        public virtual User[] Users(params string[] names)
        {
            if (names.Length > 0)
            {
                return HotCache.Cache<User>().Find(new User { }, "Username", names);
            }
            return new User[0];
        }
        public virtual void Delete(Role role)
        {
            if (String.IsNullOrEmpty(role.Rolename) == false && role.Site.HasValue)
            {
                HotCache.Cache<Role>().Delete(role);
            }
        }
        public virtual void Put(Account account)
        {
            if (String.IsNullOrEmpty(account.Name) == false)
            {
                HotCache.Cache<Account>().Put(account);

            }

        }
        public virtual Account Account(Guid user_id, int type)
        {
            return HotCache.Get(new Account { user_id = user_id, Type = type });//, 0, out index);


        }
        public virtual Account[] Account(Guid user_id)
        {
            int index;
            return HotCache.Cache<Account>().Find(new Account { user_id = user_id }, 0, out index);


        }
        public virtual Account Account(String name)
        {
            return HotCache.Cache<Account>().Get(new Entities.Account { Name = name });

        }
        public virtual Account[] Account(params String[] names)
        {

            if (names.Length > 0)
            {
                int index;
                return HotCache.Cache<Account>().Find(new Account { }, 0, out index, "Name", names);

            }
            return new Account[0];
        }

        public virtual void Delete(Authority wildcard)
        {

            HotCache.Cache<Authority>().Delete(wildcard);

        }

        public virtual Authority[] Authority(int site, params string[] wildcards)
        {
            if (wildcards.Length > 0)
            {
                return HotCache.Cache<Authority>().Find(new Entities.Authority { Site = site }, "Key", wildcards);
            }
            else
            {
                return new Authority[0];
            }

        }

        public virtual Authority Authority(int site, string wildcardKey)
        {
            return HotCache.Cache<Authority>().Get(new Entities.Authority { Key = wildcardKey, Site = site });


        }

        public virtual void Put(Authority wildcard)
        {
            if (String.IsNullOrEmpty(wildcard.Key) == false && wildcard.Site.HasValue)
            {
                HotCache.Cache<Authority>().Put(wildcard);
            }

        }

        public virtual void Put(params Menu[] menus)
        {

            foreach (var m in menus)
            {
                if (m.Site.HasValue == false)
                {
                    m.Site = 0;
                }
                HotCache.Cache<Menu>().Put(m);
            }
        }
        public virtual void Put(Menu menu)
        {
            if (menu.Id.HasValue)
            {
                if (menu.Site.HasValue == false)
                {
                    menu.Site = 0;
                }
                HotCache.Cache<Menu>().Put(menu);
            }
        }

        public virtual Menu Menu(int site, int id)
        {
            return HotCache.Cache<Menu>().Get(new Data.Entities.Menu { Site = site, Id = id });
        }

        public virtual void Delete(Menu menu)
        {
            HotCache.Cache<Menu>().Delete(menu);

        }

        public virtual Menu[] Menu(int site)
        {
            int index = 0;
            return HotCache.Cache<Menu>().Find(new Entities.Menu { Site = site }, 0, out index);

        }

        public virtual Menu[] Menus(int site, int parentId)
        {
            int index = 0;
            return HotCache.Cache<Menu>().Find(new Entities.Menu { Site = site, ParentId = parentId }, 0, out index);
        }


        public virtual void Put(params Role[] roles)
        {
            var hotM = HotCache.Cache<Role>();
            foreach (var r in roles)
            {
                if (r.Site.HasValue)
                    hotM.Put(r);
            }
        }


        public virtual Organize Organize(int org_id)
        {
            return HotCache.Cache<Organize>().Get(new Entities.Organize { Id = org_id });

        }
        public virtual Organize[] Organize(params int[] org_ids)
        {
            if (org_ids.Length > 0)
            {
                int index = 0;
                return HotCache.Cache<Organize>().Find(new Entities.Organize { }, 0, out index, "Id", org_ids).OrderBy(r => r.Seq ?? 0).ToArray();
            }
            return new Organize[0];
        }
        public virtual Organize[] Organizes(int parent_org_id)
        {
            int index = 0;
            return HotCache.Cache<Organize>().Find(new Entities.Organize { ParentId = parent_org_id }, 0, out index).OrderBy(r => r.Seq ?? 0).ToArray();

        }
        public virtual int[] OrganizesTree(Entities.User user)
        {
            int index = 0;
            var orgs = HotCache.Cache<OrganizeMember>().Find(new OrganizeMember { user_id = user.Id.Value }, 0, out index);

            var ls = new HashSet<int>();
            var cache = HotCache.Cache<Organize>();
            foreach (var org in orgs)
            {
                var org_id = org.org_id.Value;
                int c = 0;
                while (org_id != 0)
                {
                    c++;
                    var p = cache.Get(new Organize { Id = org_id });
                    if (p != null)
                    {
                        ls.Add(p.Id.Value);
                        org_id = p.ParentId ?? 0;
                    }
                    if (c > 20)
                    {
                        break;
                    }

                }
            }
            return ls.ToArray();
        }
        public virtual Organize[] Organizes(Entities.User user)
        {
            int index = 0;
            var orgs = HotCache.Cache<OrganizeMember>().Find(new OrganizeMember { user_id = user.Id.Value }, 0, out index);
            var ls = new List<int>();
            foreach (var org in orgs)
            {
                ls.Add(org.org_id.Value);

            }
            if (ls.Count > 0)
            {
                return HotCache.Cache<Organize>().Find(new Organize { }, 0, out index, "Id", ls);
            }
            else
            {
                return new Organize[0];
            }
        }
        public virtual void Put(OrganizeMember item)
        {
            if (item.org_id.HasValue && item.user_id.HasValue)
            {
                HotCache.Cache<OrganizeMember>().Put(item);
            }
        }
        public virtual void Put(Organize item)
        {
            if (item.Id.HasValue)
            {
                HotCache.Cache<Organize>().Put(item);
            }
        }
    }
}
