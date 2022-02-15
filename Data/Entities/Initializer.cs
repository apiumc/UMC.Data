using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data.Sql;
using UMC.Net;

namespace UMC.Data.Entities
{
    public class Initializer : UMC.Data.Sql.Initializer
    {
        public override string ProviderName => "defaultDbProvider";

        public override string Caption => "UMC基础";

        public override string Name => "UMC";

        public override bool Resource(NetContext context, string targetKey)
        {
            if (targetKey.EndsWith("UMC.js") || targetKey.EndsWith("Page.js"))
            {
                context.Output.WriteLine("UMC.UI.Config({'posurl': '/UMC/' + (UMC.cookie('device') || UMC.cookie('device', UMC.uuid()))}); ");


                return true;
            }
            return base.Resource(context, targetKey);
        }

        public Initializer()
        {
            this.Setup(new Account() { user_id = Guid.Empty, Type = 0 }, new Account { ConfigData = String.Empty });
            this.Setup(new Cache() { Id = Guid.Empty, CacheKey = String.Empty }, new Cache { CacheData = String.Empty });
            this.Setup(new Session { SessionKey = String.Empty }, new Session { Content = String.Empty });
            this.Setup(new Log (), new Log() { Context = String.Empty });
            this.Setup(new Number() { CodeKey = String.Empty });
            this.Setup(new User() { Username = String.Empty });
            this.Setup(new Password() { Key = Guid.Empty });
            this.Setup(new Organize() { Id = Guid.Empty });
            this.Setup(new OrganizeMember() { org_id = Guid.Empty, user_id = Guid.Empty });
            this.Setup(new Wildcard() { WildcardKey = String.Empty }, new Wildcard { Authorizes = String.Empty });
            this.Setup(new Picture() { group_id = Guid.Empty });
            this.Setup(new UserToRole() { Rolename = String.Empty, user_id = Guid.Empty });
            this.Setup(new Role() { Rolename = String.Empty });
            this.Setup(new Config() { ConfKey = String.Empty }, new Config { ConfValue = String.Empty });
            this.Setup(new Click() { Code = 0 });
            this.Setup(new Menu() { Id = Guid.Empty });
            //this.Setup(new Location() { Id = 0 });
            this.Setup(new SearchKeyword { user_id = Guid.Empty, Keyword = String.Empty });



        }
        public override void Setup(IDictionary hash)
        {
            var adminRole = new Role()
            {
                Rolename = UMC.Security.Membership.AdminRole,
                Explain = "管理员"
            };
            Data.DataFactory.Instance().Put(adminRole, new Role
            {
                Rolename = UMC.Security.Membership.UserRole,
                Explain = "员工账户"
            }, new Role
            {
                Rolename = UMC.Security.Membership.GuestRole,
                Explain = "来客"
            });
            var m = UMC.Security.Membership.Instance();
            m.CreateUser("admin", "管理员");
            m.Password("admin", "admin");
            m.AddRole("admin", UMC.Security.Membership.AdminRole);



        }

        public override void Menu(IDictionary hash)
        {
            Data.DataFactory.Instance().Put(new Menu()
            {
                Icon = "\uF07c",
                Caption = "静态资源",
                IsDisable = false,
                ParentId = Guid.Empty,
                Seq = 92,
                Id = Utility.Guid("#static", true),
                Url = "#static"
            }, new Menu()
            {
                Icon = "\uf0ae",
                Caption = "菜单管理",
                IsDisable = false,
                ParentId = Guid.Empty,
                Seq = 93,
                Id = Utility.Guid("#menu", true),
                Url = "#menu"
            }, new Menu()
            {
                Icon = "\uf0c0",
                Caption = "用户管理",
                IsDisable = false,
                ParentId = Guid.Empty,
                Seq = 94,
                Id = Utility.Guid("#user", true),
                Url = "#user"
            });
        }
    }
}
