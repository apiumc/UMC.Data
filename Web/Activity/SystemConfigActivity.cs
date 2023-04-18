using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Data;
using UMC.Data.Sql;
using UMC.Web;

namespace UMC.Web.Activity
{
    class SystemConfigActivity : UMC.Web.WebActivity
    {
        public override void ProcessActivity(UMC.Web.WebRequest request, UMC.Web.WebResponse response)
        {
            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能配置");
            }
            var config = this.AsyncDialog("Config", g => new UITextDialog() { Title = "请输入配置的节点" });

            var cfg = UMC.Data.Reflection.Configuration(config.ToLower());
            var configKey = this.AsyncDialog("Key", g =>
            {
                var form = request.SendValues ?? new UMC.Web.WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta(request.Arguments))
                        .RefreshEvent($"{request.Model}.{request.Command}")
                            .Builder(), true);
                }
                var title = new UITitle(config);
                switch (config.ToLower())
                {
                    case "assembly":
                        title.Title = "处理类配置";
                        break;
                    case "database":
                        title.Title = "数据库配置";
                        break;
                    case "umc":
                        title.Title = "云模块配置";
                        break;
                    case "parser":
                        title.Title = "转码配置";
                        break;
                    case "account":
                        title.Title = "账户配置";
                        break;
                    case "payment":
                        title.Title = "支付配置";
                        break;
                }

                var ui = UISection.Create(title);
                var key = this.AsyncDialog("Type", "FILES");


                if (key == "FILES")
                {
                    ui.Title.Right(new UIEventText("新建").Click(new UIClick("Config", config, g, "NEW").Send(request.Model, request.Command)));
                    var ui2 = ui.NewSection();
                    for (var i = 0; i < cfg.Count; i++)
                    {
                        var p = cfg[i];
                        var cell = UICell.UI(p.Name, "", UIClick.Query(new WebMeta("Type", String.Format("{0}", p.Name))));
                        ui2.Delete(cell, new UIEventText().Click(new UIClick(new WebMeta(request.Arguments).Put(g, "Del", "Name", p.Name)).Send(request)));
                    }
                }
                else
                {
                    var p = cfg[key];
                    ui.Title.Right(new UIEventText("新建").Click(new UIClick("Config", config, g, p.Name + "$NEW").Send(request.Model, request.Command)));
                    ui.AddCell('\uf112', "上一层", String.Empty, UIClick.Query(new WebMeta("Type", "FILES")));
                    ui.AddCell("节点", p.Name);
                    ui.AddCell("类型", p.Type);
                    var ui2 = ui.NewSection();
                    for (var i = 0; i < p.Attributes.Count; i++)
                    {
                        var cell = UICell.UI(p.Attributes.GetKey(i), new UIClick("Config", config, g, String.Format("{0}${1}", p.Name, p.Attributes.GetKey(i))).Send(request.Model, request.Command));
                        ui2.Delete(cell, new UIEventText().Click(new UIClick(new WebMeta(request.Arguments).Put(g, "Del", "Name", String.Format("{0}${1}", p.Name, p.Attributes.GetKey(i)))).Send(request)));

                    }
                }


                response.Redirect(ui);
                return this.DialogValue("none");
            });
            switch (configKey)
            {
                case "Del":
                    {
                        var name = this.AsyncDialog("Name", "none");

                        var names = name.Split('$');
                        if (names.Length == 1)
                        {
                            cfg.Remove(name);
                        }
                        else
                        {
                            var p2 = cfg[names[0]];
                            if (p2 != null)
                            {
                                p2.Attributes.Remove(names[1]);
                            }
                        }
                        Reflection.Configuration(config.ToLower(), cfg);
                        this.Context.Send($"{request.Model}.{request.Command}", true);

                    }
                    break;
                case "NEW":
                    {
                        var ps = this.AsyncDialog("Setting", g =>
                        {
                            var fm = new UIFormDialog();
                            fm.Title = "新建节点";
                            fm.AddText("节点名", "Name", String.Empty);
                            fm.AddText("类型值", "Value", String.Empty);

                            fm.Submit("确认", $"{request.Model}.{request.Command}");
                            return fm;
                        });

                        var pro2 = Provider.Create(ps["Name"], ps["Value"]);
                        var p2 = cfg[pro2.Name];
                        if (p2 != null)
                        {
                            cfg.Remove(p2.Name);
                            pro2.Attributes.Add(p2.Attributes);
                        }

                        cfg.Add( pro2);
                        Reflection.Configuration(config.ToLower(), cfg);
                        this.Context.Send($"{request.Model}.{request.Command}", false);
                        response.Redirect(request.Model, request.Command, $"{config.ToLower()}${pro2.Name}", true);
                    }
                    break;
                default:
                    var ckeys = configKey.Split('$');

                    if (ckeys.Length == 2)
                    {
                        var pro = cfg[ckeys[0]];
                        var ps = this.AsyncDialog("Setting", g =>
                         {
                             var fm = new UIFormDialog();
                             fm.Title = ckeys[1] + "配置";

                             if (ckeys[1] == "NEW")
                             {
                                 fm.AddText("新建属性名", "Name", String.Empty);
                                 fm.AddText("新建属性值", "Value", String.Empty);
                             }
                             else
                             {

                                 fm.AddTextValue().Add("属性名", ckeys[1]);
                                 fm.AddTextarea("属性值", "Value", pro[ckeys[1]]);
                             }
                             fm.Submit("确认", $"{request.Model}.{request.Command}");
                             return fm;
                         });
                        var value = ps["Value"];
                        pro.Attributes[ps["Name"] ?? ckeys[1]] = value;

                        Reflection.Configuration(config.ToLower(), cfg);
                        this.Context.Send($"{request.Model}.{request.Command}", true);
                    }
                    break;
            }


        }
    }
}