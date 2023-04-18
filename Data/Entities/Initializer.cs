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

        public override string Caption => "UMC基础";

        public override string Name => "UMC";
        
        public override void Setup(CSV.Log log)
        {
            var m = UMC.Security.Membership.Instance();
            m.CreateUser("admin", "管理员");
            m.Password("admin", "admin");
            m.AddRole("admin", 0, UMC.Security.Membership.AdminRole);

            var idValue = 100;
            Data.DataFactory.Instance().Put(new Menu()
            {
                Site = 0,
                Icon = "\uF07c",
                Caption = "静态资源",
                IsHidden = false,
                ParentId = 0,
                Seq = 92,
                Id = idValue++,
                Url = "#static"
            }, new Menu()
            {
                Site = 0,
                Icon = "\uf0ae",
                Caption = "菜单管理",
                IsHidden = false,
                ParentId = 0,
                Seq = 93,
                Id = idValue++,
                Url = "#menu"
            }, new Menu()
            {
                Site = 0,
                Icon = "\uf0c0",
                Caption = "组织账户",
                IsHidden = false,
                ParentId = 0,
                Seq = 94,
                Id = idValue++,
                Url = "#organize"
            }, new Menu()
            {
                Site = 0,
                Icon = "\uea05",
                Caption = "功能授权",
                IsHidden = false,
                ParentId = 0,
                Seq = 94,
                Id = idValue++,
                Url = "#authority"
            });
            log.Info("默认账户:admin 密码:admin");
        }
    }
}
