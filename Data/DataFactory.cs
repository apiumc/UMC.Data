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
            HotCache.Register<Config>("ConfKey");
            HotCache.Register<Wildcard>("WildcardKey").IsFast = true;
            HotCache.Register<Password>("Key");
            HotCache.Register<Session>("SessionKey").Register("user_id");

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
            return Database.Instance().ObjectEntity<Session>()
                     .Where.And().Equal(new Session
                     {
                         user_id = user_id,
                     }).Entities.Query();
            //return HotCache.Cache<Session>().Get(new Data.Entities.Session(), "user_id", user_id);
        }
        public virtual Config Config(string key)
        {
            return HotCache.Cache<Config>().Get(new Config { ConfKey = key });
        }
        public virtual void Put(Config cache)
        {
            HotCache.Cache<Config>().Put(cache);
        }
        public virtual String Password(Guid key)
        {
            var password = HotCache.Cache<Password>().Get(new Password { Key = key });

            if (password != null && password.Body != null)
            {
                return UMC.Data.Utility.DES(password.Body, password.Key.Value);
            }
            return null;

        }
        public virtual void Delete(Password password)
        {
            if (password.Key.HasValue)
            {
                HotCache.Cache<Password>().Delete(new Password
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
            Database.Instance().ObjectEntity<Data.Entities.Cache>().Where.And().Equal(new Data.Entities.Cache
            {
                Id = cache.Id.Value,
                CacheKey = cache.CacheKey ?? String.Empty
            }).Entities.Delete();
        }
        public virtual void Delete(Organize organize)
        {
            Database.Instance().ObjectEntity<Data.Entities.Organize>().Where.And().Equal(new Data.Entities.Organize
            {
                Id = organize.Id
            }).Entities.Delete();
        }
        public virtual void Delete(OrganizeMember organizeMember)
        {
            Database.Instance().ObjectEntity<Data.Entities.OrganizeMember>().Where.And().Equal(new Data.Entities.OrganizeMember
            {
                user_id = organizeMember.user_id.Value,
                org_id = organizeMember.org_id.Value
            }).Entities.Delete();
        }

        public virtual void Delete(Picture picture)
        {
            Database.Instance().ObjectEntity<Data.Entities.Picture>().Where.And().Equal(new Picture { group_id = picture.group_id.Value }).Entities.Delete();
        }
        public virtual void Put(SearchKeyword keyword)
        {
            if (String.IsNullOrEmpty(keyword.Keyword) == false && keyword.user_id.HasValue)
            {
                Database.Instance().ObjectEntity<Data.Entities.SearchKeyword>().Where.And()
                    .Equal(new SearchKeyword { Keyword = keyword.Keyword, user_id = keyword.user_id })
                    .Entities.IFF(e => e.Update(keyword) == 0, e => e.Insert(keyword));

                Database.Instance().ObjectEntity<Data.Entities.SearchKeyword>().Where.And()
                   .Equal(new SearchKeyword { Keyword = keyword.Keyword, user_id = Guid.Empty })
                   .Entities.IFF(e => e.Update("{0}+{1}", new SearchKeyword { Time = 1 }) == 0, e =>
                   {
                       keyword.Time = 1;
                       keyword.user_id = Guid.Empty;
                       e.Insert(keyword);
                   });


            }
        }
        public virtual SearchKeyword[] SearchKeyword(Guid userId)
        {

            return Database.Instance().ObjectEntity<Data.Entities.SearchKeyword>().Where.And()
                   .Equal(new SearchKeyword { user_id = userId }).Entities.Order.Desc(new SearchKeyword { Time = 0 })
                   .Entities.Query(0, 20);



        }

        public virtual Cache Get(Guid id, string cacheKey)
        {
            return Database.Instance().ObjectEntity<Data.Entities.Cache>().Where.And().Equal(new Data.Entities.Cache
            {
                Id = id,
                CacheKey = cacheKey ?? String.Empty
            }).Entities.Single();
        }


        public virtual Picture[] Pictures(params Guid[] gid)
        {
            if (gid.Length > 0)
            {
                return Database.Instance().ObjectEntity<Data.Entities.Picture>().Where.And().In(new Data.Entities.Picture
                {
                    group_id = gid[0]
                }, gid).Entities.Query();
            }
            return new Picture[0];
        }

        public virtual Picture Picture(Guid guid)
        {

            return Database.Instance().ObjectEntity<Data.Entities.Picture>().Where.And().Equal(new Data.Entities.Picture
            {
                group_id = guid
            }).Entities.Single();

        }



        public virtual void Put(Cache cache)
        {
            Database.Instance().ObjectEntity<Data.Entities.Cache>().Where.And().Equal(new Data.Entities.Cache
            {
                Id = cache.Id.Value,
                CacheKey = cache.CacheKey ?? String.Empty
            }).Entities.IFF(e => e.Update(cache) == 0, e => e.Insert(cache));
        }

        public virtual void Put(Picture picture)
        {
            Database.Instance().ObjectEntity<Data.Entities.Picture>().Where.And().Equal(new Data.Entities.Picture
            {
                group_id = picture.group_id.Value
            }).Entities.IFF(e => e.Update(picture) == 0, e => e.Insert(picture));
        }
        public virtual User[] Search(User search, out int total, int start, int limit)
        {
            var entity = Database.Instance().ObjectEntity<Data.Entities.User>();

            if (search.Flags.HasValue)
            {
                entity.Where.And("(Flags&{0})={0}", search.Flags).And().Contains().Or().Like(new UMC.Data.Entities.User
                {
                    Username = search.Username,
                    Alias = search.Alias
                });
            }
            else
            {
                entity.Where.Contains().Or().Like(new UMC.Data.Entities.User { Username = search.Username, Alias = search.Alias });

            }

            total = entity.Count();
            if (total == 0)
            {
                return new Entities.User[0];
            }
            return entity.Query(start, limit);

        }
        public virtual bool Put(User user)
        {
            if (user.Id.HasValue)
            {
                bool s = true;
                Database.Instance().ObjectEntity<Data.Entities.User>().Where.And().Equal(new Data.Entities.User
                {
                    Id = user.Id.Value
                }).Entities.IFF(e => e.Update(UMC.Data.Reflection.PropertyToDictionary(user)) == 0,
                    e =>
                    {
                        if (String.IsNullOrEmpty(user.Username) == false)
                        {
                            e.Insert(user);
                        }
                        else
                        {
                            s = false;
                        }
                    });
                return s;
            }
            else if (String.IsNullOrEmpty(user.Username) == false)
            {
                return Database.Instance().ObjectEntity<Data.Entities.User>().Where.And().Equal(new Data.Entities.User
                {
                    Username = user.Username
                }).Entities.Update(UMC.Data.Reflection.PropertyToDictionary(user)) > 0;
            }
            return false;
        }

        public virtual void Put(Role role)
        {
            Database.Instance().ObjectEntity<Data.Entities.Role>().Where.And().In(new Data.Entities.Role
            {
                Rolename = role.Rolename
            }).Entities.IFF(e => e.Update(role) == 0, e => e.Insert(role));
        }

        public virtual void Put(Guid userid, params Role[] roles)
        {
            var rEntity = Database.Instance().ObjectEntity<Data.Entities.UserToRole>().Where.And().In(new Data.Entities.UserToRole
            {
                user_id = userid
            }).Entities;
            var ls = new List<UserToRole>();
            foreach (var v in roles)
            {
                ls.Add(new UserToRole { Rolename = v.Rolename, user_id = userid });

            }
            rEntity.Delete();
            if (ls.Count > 0)
            {
                rEntity.Insert(ls.ToArray());

            }
        }

        public virtual Role Role(string rolename)
        {
            return Database.Instance().ObjectEntity<Data.Entities.Role>().Where.And().In(new Data.Entities.Role
            {
                Rolename = rolename
            }).Entities.Single();
        }
        public virtual Role[] Roles()
        {
            return Database.Instance().ObjectEntity<Data.Entities.Role>().Query();
        }


        public virtual Role[] Roles(Guid userid)
        {
            var rls = new List<Role>();
            Database.Instance().ObjectEntity<Data.Entities.UserToRole>().Where.And().Equal(new UserToRole { user_id = userid }).Entities
                .Query(r =>
                {
                    rls.Add(new Entities.Role { Rolename = r.Rolename });
                });

            return rls.ToArray();
        }

        public virtual User User(string username)
        {
            return Database.Instance().ObjectEntity<Data.Entities.User>().Where.And().In(new Data.Entities.User
            {
                Username = username
            }).Entities.Single();
        }

        public virtual User User(Guid userId)
        {
            return Database.Instance().ObjectEntity<Data.Entities.User>().Where.And().In(new Data.Entities.User
            {
                Id = userId
            }).Entities.Single();
        }

        public virtual User[] Users(params Guid[] userIds)
        {
            return Database.Instance().ObjectEntity<Data.Entities.User>().Where.And().In(new Data.Entities.User
            {
                Id = userIds[0]
            }, userIds).Entities.Query();
        }
        public virtual User[] Users(Data.Entities.Organize organize)
        {
            var ids = new List<Guid>();
            Database.Instance().ObjectEntity<Data.Entities.OrganizeMember>()
                .Where.And().In(new OrganizeMember { org_id = organize.Id.Value }).Entities
                .Order.Asc(new OrganizeMember { Seq = 0 }).Entities.Query(dr => ids.Add(dr.user_id.Value));
            if (ids.Count > 0)
            {
                return Users(ids.ToArray()).OrderBy(r => ids.IndexOf(r.Id.Value)).ToArray();
            }
            return new User[0];

        }
        public virtual User[] Users(params string[] names)
        {
            return Database.Instance().ObjectEntity<Data.Entities.User>().Where.And().In(new Data.Entities.User
            {
                Username = names[0]
            }, names).Entities.Query();
        }
        public virtual void Delete(Role role)
        {
            if (String.IsNullOrEmpty(role.Rolename) == false)
            {
                Database.Instance().ObjectEntity<Data.Entities.Role>().Where.And().In(new Data.Entities.Role
                {
                    Rolename = role.Rolename
                }).Entities.Delete();
            }
        }
        public virtual void Put(Account account)
        {
            if (String.IsNullOrEmpty(account.Name) == false)
            {
                Database.Instance().ObjectEntity<Data.Entities.Account>().Where.And().In(new Data.Entities.Account
                {
                    Name = account.Name
                }).Entities.IFF(e => e.Update(account) == 0, e =>
                {
                    e.Insert(account);
                });
            }

        }
        public virtual Account[] Account(Guid user_id)
        {
            return Database.Instance().ObjectEntity<Data.Entities.Account>().Where.And().Equal(new Data.Entities.Account
            {
                user_id = user_id
            }).Entities.Query();

        }
        public virtual Account Account(String name, int type)
        {

            return Database.Instance().ObjectEntity<Data.Entities.Account>().Where.And().Equal(new Data.Entities.Account
            {
                Name = name,
                Type = type
            }).Entities.Single();
        }
        public virtual Account[] Account(params String[] names)
        {

            if (names.Length > 0)
            {
                return Database.Instance().ObjectEntity<Data.Entities.Account>().Where.And().In(new Data.Entities.Account
                {
                    Name = names[0]
                }, names).Entities.Query();
            }
            return new Account[0];
        }

        public virtual void Delete(Wildcard wildcard)
        {
            if (String.IsNullOrEmpty(wildcard.WildcardKey) == false)
            {
                HotCache.Cache<Wildcard>().Delete(new Entities.Wildcard { WildcardKey = wildcard.WildcardKey });
            }
        }

        public virtual Wildcard[] Wildcard(params string[] wildcards)
        {
            var list = new List<Wildcard>();
            foreach (var key in wildcards)
            {
                var wild = HotCache.Cache<Wildcard>().Get(new Entities.Wildcard { WildcardKey = key });
                if (wild != null)
                {
                    list.Add(wild);
                }
            }
            return list.ToArray();

        }

        public virtual Wildcard Wildcard(string wildcardKey)
        {
            return HotCache.Cache<Wildcard>().Get(new Entities.Wildcard { WildcardKey = wildcardKey });


        }

        public virtual void Put(Wildcard wildcard)
        {
            HotCache.Cache<Wildcard>().Put(wildcard);

        }
        public virtual void Post(Session session)
        {

            HotCache.Remove(session);
            HotCache.Synchronize<Session>(new String[] { "SessionKey" }, new object[] { session.SessionKey });
            UMC.Data.Database.Instance().ObjectEntity<Session>()
            .Where.And().Equal(new Session
            {
                SessionKey = session.SessionKey
            }).Entities
            .IFF(e => e.Update(session) == 0, e => e.Insert(session));

        }
        public virtual ProviderConfiguration Configuration(string configKey)
        {
            var file = Reflection.AppDataPath(String.Format("UMC\\{0}.xml", configKey.ToLower()));
            return ProviderConfiguration.GetProvider(file);
        }
        public virtual void Configuration(string configKey, ProviderConfiguration providerConfiguration)
        {
            var file = Reflection.AppDataPath(String.Format("UMC\\{0}.xml", configKey.ToLower()));

            providerConfiguration.WriteTo(file);
        }

        public virtual void Put(params Menu[] menus)
        {
            menus.Any(d =>
            {
                if (d.Site.HasValue == false)
                {
                    d.Site = 0;
                }
                return false;
            });

            Data.Database.Instance().ObjectEntity<Menu>().Insert(menus);//.Commit();
        }
        public virtual Click Click(int code)
        {
            var storeEntity = Data.Database.Instance().ObjectEntity<Data.Entities.Click>();
            return storeEntity.Where.And().Equal(new Data.Entities.Click { Code = code }).Entities.Single();
        }
        public virtual void Put(Click click)
        {
            var storeEntity = Data.Database.Instance().ObjectEntity<Data.Entities.Click>();
            storeEntity.Where.And().Equal(new Data.Entities.Click { Code = click.Code });
            //var c = new Data.Entities.Click
            //{
            //    Query = Data.JSON.Serialize(ob),
            //    Code = click.,
            //    Quality = qty
            //};
            storeEntity.IFF(e => e.Update(click) == 0, e => e.Insert(click));
        }
        public virtual void Delete(Click click)
        {
            var storeEntity = Data.Database.Instance().ObjectEntity<Data.Entities.Click>();
            storeEntity.Where.And().Equal(new Data.Entities.Click { Code = click.Code }).Entities.Delete();
        }
        public virtual Number Number(string codeKey)
        {
            var storeEntity = Data.Database.Instance().ObjectEntity<Data.Entities.Number>();
            return storeEntity.Where.And().Equal(new Data.Entities.Number { CodeKey = codeKey }).Entities.Single();
        }

        public virtual void Put(Number number)
        {

            Data.Database.Instance().ObjectEntity<Number>().Where.And().Equal(new Data.Entities.Number { CodeKey = number.CodeKey })
               .Entities.IFF(e => e.Update(number) == 0, e => e.Insert(number));


        }
        public virtual void Put(Menu menu)
        {
            if (menu.Id.HasValue)
            {

                Data.Database.Instance().ObjectEntity<Menu>().Where.And().Equal(new Data.Entities.Menu { Id = menu.Id })
                   .Entities.IFF(e => e.Update(menu) == 0, e =>
                   {
                       if (menu.Site.HasValue == false)
                       {
                           menu.Site = 0;
                       }
                       e.Insert(menu);
                   });
            }
        }

        public virtual Menu Menu(Guid id)
        {
            return Data.Database.Instance().ObjectEntity<Menu>().Where.And().Equal(new Data.Entities.Menu { Id = id })
                     .Entities.Single();
        }

        public virtual void Delete(Menu menu)
        {
            if (menu.Id.HasValue || menu.Site.HasValue || menu.ParentId.HasValue)
            {
                Data.Database.Instance().ObjectEntity<Menu>().Where.And().Equal(new Data.Entities.Menu { Id = menu.Id, ParentId = menu.ParentId, Site = menu.Site })
                    .Entities.Delete();
            }
        }

        public virtual Menu[] Menu(int site)
        {
            return Data.Database.Instance().ObjectEntity<Menu>().Where.And().Equal(new Data.Entities.Menu { Site = site })
                     .Entities.Query();
        }

        public virtual Menu[] Menu(Guid parentId, int site)
        {
            return Data.Database.Instance().ObjectEntity<Menu>().Where.And().Equal(new Data.Entities.Menu { ParentId = parentId, Site = site })
                     .Entities.Query();
        }

        public virtual Menu[] Menu(string searchKey, int type)
        {
            return Data.Database.Instance().ObjectEntity<Menu>().Where.And().Equal(new Data.Entities.Menu { Site = type })
                .And().Like(new Menu { Caption = searchKey })
                     .Entities.Query();
        }

        public virtual void Put(params Role[] roles)
        {
            Database.Instance().ObjectEntity<Data.Entities.Role>().Insert(roles);
        }


        public virtual Organize Organize(Guid org_id)
        {

            return Database.Instance().ObjectEntity<Organize>()
                         .Where.And().Equal(new Organize
                         {
                             Id = org_id
                         }).Entities.Single();
        }
        public virtual Organize[] Organize(params Guid[] org_id)
        {
            if (org_id.Length > 0)
            {

                return Database.Instance().ObjectEntity<Organize>()
                         .Where.And().In(new Organize
                         {
                             Id = org_id[0]
                         }, org_id).Entities.Query().OrderBy(r => r.Seq ?? 0).ToArray();



            }
            return new Organize[0];
        }
        public virtual Organize[] Organizes(Guid parent_org_id)
        {

            return Database.Instance().ObjectEntity<Organize>()
                         .Where.And().In(new Organize
                         {
                             ParentId = parent_org_id
                         }).Entities.Query().OrderBy(r => r.Seq ?? 0).ToArray();


        }
        public virtual Organize[] Organizes(Entities.User user)
        {


            return Database.Instance().ObjectEntity<Organize>()
                         .Where.And().In("Id",
                         Database.Instance().ObjectEntity<OrganizeMember>().Where.And()
                         .In(new OrganizeMember { user_id = user.Id.Value }).Entities.Script(new OrganizeMember { org_id = Guid.Empty })
                         ).Entities.Query().OrderBy(r => r.Seq ?? 0).ToArray();

        }
        public virtual void Put(OrganizeMember item)
        {
            if (item.org_id.HasValue && item.user_id.HasValue)
            {
                Database.Instance().ObjectEntity<OrganizeMember>()
               .Where.And().Equal(new OrganizeMember
               {
                   user_id = item.user_id.Value,
                   org_id = item.org_id.Value
               }).Entities.IFF(e => e.Update(item) == 0, e => e.Insert(item));
            }
        }
        public virtual void Put(Organize item)
        {
            if (item.Id.HasValue)
            {
                Database.Instance().ObjectEntity<Organize>()
               .Where.And().Equal(new Organize
               {
                   Id = item.Id.Value,
               }).Entities.IFF(e => e.Update(item) == 0, e => e.Insert(item));
            }
        }
        /// <summary>
        /// 数据同步
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="data"></param>
        public virtual void SynchData(byte flag, string data)
        {

        }
        public virtual void Put(Log log)
        {
            var writer = new System.IO.StringWriter();
            UMC.Data.CSV.WriteLine(writer, log.Key, log.Path, log.Username, log.IP, log.Duration, log.Referrer, log.UserAgent, log.Time, log.Status, log.Context);

            this.SynchData(0x02, writer.ToString());

        }
    }
}
