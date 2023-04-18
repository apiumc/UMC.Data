using System;
using System.Collections;
using System.Collections.Generic;
namespace UMC.Security
{

    /// <summary>
    /// 账户
    /// </summary>
    public class Account
    {

        public string ForId
        {
            get;
            private set;
        }

        /// <summary>
        /// 用户Mail
        /// </summary>
        public const int EMAIL_ACCOUNT_KEY = 1;
        /// <summary>
        /// 移动电话
        /// </summary>
        public const int MOBILE_ACCOUNT_KEY = 2;


        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid user_id
        {
            get;
            private set;
        }
        public int Type
        {
            get;
            private set;
        }
        /// <summary>
        /// 账户名
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
        /// <summary>
        /// 账户标签
        /// </summary>
        public UMC.Security.UserFlags Flags
        {
            get;
            private set;
        }
        private Account(UMC.Data.Entities.Account acc)
        {
            this.Name = acc.Name;
            this.Flags = acc.Flags ?? UMC.Security.UserFlags.Normal;
            this.ForId = acc.ForId;
            this.user_id = acc.user_id.Value;
            this.Type = acc.Type ?? 0;
            this.Items = UMC.Data.JSON.Deserialize<Web.WebMeta>(acc.ConfigData) ?? new Web.WebMeta();
        }

        public static Account Create(UMC.Data.Entities.Account acc)
        {
            return new Account(acc);
        }
        /// <summary>
        /// 更改数据配置
        /// </summary>
        public void Commit()
        {
            UMC.Data.DataFactory.Instance().Put(new UMC.Data.Entities.Account
            {
                ConfigData = Data.JSON.Serialize(this.Items),
                ForId = this.ForId,
                Name = this.Name,
                user_id = this.user_id,
                Type = this.Type
            });
        }

        /// <summary>
        /// 数据
        /// </summary>
        public Web.WebMeta Items
        {
            get;
            private set;
        }

        public Account Put(String key ,string value)
        {
            this.Items.Put(key, value);
            return this;

        }
        /// <summary>
        /// 获得关系
        /// </summary>
        /// <param name="main"></param>
        /// <param name="relations"></param>
        public static void GetRelation(Data.Entities.Account main, params Data.Entities.Account[] relations)
        {
            if (String.IsNullOrEmpty(main.Name) || main.Type.HasValue == false)
            {
                throw new ArgumentException("main 中的属性name和Type必须有值");
            }
            var names = new List<String>();
            names.Add(main.Name);
            var types = new List<int>();
            types.Add(main.Type.Value);
            foreach (var r in relations)
            {
                if (r.Type.HasValue)
                    types.Add(r.Type.Value);
                if (String.IsNullOrEmpty(r.Name) == false)
                {
                    names.Add(r.Name);
                }
            }


            var mids = new List<Guid>();
            Guid mid = Guid.NewGuid();
            var rels = new List<Data.Entities.Account>();


            UMC.Data.Utility.Each(Data.DataFactory.Instance().Account(names.ToArray()), g =>
             {
                 if (types.Exists(t => t == g.Type))
                 {
                     rels.Add(g);
                     if (g.user_id.Value != mid)
                     {
                         mid = g.user_id.Value;
                         mids.Add(mid);
                     }
                 }

             });

            if (mids.Count > 0)
            {
                foreach (var m in mids)
                {
                    UMC.Data.Utility.Each(Data.DataFactory.Instance().Account(m), g =>
                    {
                        if (types.Exists(t => t == g.Type))
                        {
                            rels.Add(g);
                        }

                    });
                }
            }
            var memberId = Guid.Empty;
            var orel = rels.FindAll(g => g.Type == main.Type);

            var om = orel.Find(g => String.Equals(main.Name, g.Name) && g.Type == main.Type);
            if (om == null)
            {
                if (relations.Length > 0)
                {
                    var rel = new List<Data.Entities.Account>(relations).Find(g => String.IsNullOrEmpty(g.Name) == false);
                    if (rel != null)
                    {
                        var v = rels.Find(g => String.Equals(g.Name, rel.Name) && rel.Type == g.Type);
                        if (v != null)
                        {
                            main.user_id = v.user_id;
                        }
                    }
                }
                if ((main.user_id ?? Guid.Empty) == Guid.Empty)
                {
                    main.user_id = Guid.NewGuid();
                }
                main.Flags = UMC.Security.UserFlags.Normal;
                Data.DataFactory.Instance().Put(main);
            }
            else
            {
                if (main.user_id.HasValue)
                {
                    if (main.user_id.Value != om.user_id.Value)
                    {
                        Data.DataFactory.Instance().Put(main);
                    }
                }
                else
                {
                    main.user_id = om.user_id;
                }
            }
            memberId = main.user_id.Value;
            foreach (var r in relations)
            {
                var mrel = rels.FindAll(g => g.Type == r.Type && memberId == g.user_id);
                if (String.IsNullOrEmpty(r.Name) == false)
                {
                    var mcards = mrel.FindAll(g => String.Equals(r.Name, g.Name, StringComparison.CurrentCultureIgnoreCase));

                    switch (mcards.Count)
                    {
                        case 0:
                            r.user_id = memberId;


                            Data.DataFactory.Instance().Put(r);
                            //entity.Insert(r);
                            mrel.Add(r);
                            break;
                        default:

                            r.Flags = UMC.Security.UserFlags.Normal;
                            r.user_id = mcards[0].user_id;
                            r.Name = mcards[0].Name;
                            r.user_id = mcards[0].user_id;
                            r.ConfigData = mcards[0].ConfigData;
                            break;
                    }
                }
                else
                {
                    var m = mrel.Find(g => r.Type == g.Type && memberId == g.user_id);
                    if (m != null)
                    {
                        r.user_id = m.user_id;
                        r.Name = m.Name;
                        r.Flags = m.Flags;
                        r.ConfigData = m.ConfigData;
                    }
                }

            }

        }
        /// <summary>
        /// 验证Key
        /// </summary>
        public const string KEY_VERIFY_FIELD = "VerifyCode";

        /// <summary>
        /// 提交新账户，如果不存在，则添加，如果存在则修改
        /// </summary>
        /// <param name="name">账户名</param>
        /// <param name="userid">用户Id</param>
        /// <param name="flags">账户标示</param>
        /// <param name="accountType">账户类型</param>
        /// <returns></returns>
        public static Account Post(string name, Guid userid, UMC.Security.UserFlags flags, int accountType)
        {

            var acc = new UMC.Data.Entities.Account
            {
                Name = name,
                user_id = userid,
                Type = accountType,
                Flags = flags
            };
            UMC.Data.DataFactory.Instance().Put(acc);
            return new Account(acc);
        }
       UMC.Data.Entities.Account[] accounts;

        public Account this[int accountType]
        {
            get
            {
                if (accounts == null)
                {
                    this.accounts =  Data.DataFactory.Instance().Account(this.user_id);
                }

                for (var i = 0; i < this.accounts.Length; i++)
                {
                    var ac = this.accounts[i];
                    if (ac.Type == accountType)
                    {
                        var act = new Account(ac);
                        act.accounts = this.accounts;
                        return act;
                    }
                }
                return null;
            }
        }
        public static Account Create(Guid userid)
        {
            return new Account(new Data.Entities.Account { user_id = userid });
        }

    }

}