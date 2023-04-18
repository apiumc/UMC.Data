using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using UMC.Data;
using UMC.Net;
using UMC.Web;

namespace UMC.Web.Activity
{
    class SystemWebActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            if (request.IsMaster == false)
            {
                this.Prompt("需要管理员权限");
            }

            var svalue = this.AsyncDialog("Key", d =>
            {
                var form = request.SendValues ?? new WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command)
                        .RefreshEvent($"{request.Model}.{request.Command}")
                        .Builder(), true);

                }
                var title = UITitle.Create();

                title.Title = "应用安全";

                var ui = UISection.Create(title);
                var webr = UMC.Data.WebResource.Instance();

                var appID = webr.Provider["appId"];
                var appSecret = webr.Provider["appSecret"];
                ui.AddCell("授权码", String.IsNullOrEmpty(appID) ? "去授权" : "去查看", new UIClick().Send(request.Model, "License"));
                ui.AddCell("检验码", String.IsNullOrEmpty(appSecret) ? "未设置" : "已设置", new UIClick().Send(request.Model, "License"));

                var cfg2 = Reflection.Configuration("UMC");

                var u2 = ui.NewSection();
                u2.Header.Put("text", "云模块");
                for (var i = 0; i < cfg2.Count; i++)
                {
                    var p = cfg2[i];
                    var cmd = p.Type;

                    if (String.IsNullOrEmpty(p.Type))
                    {
                        cmd = "*";
                    }
                    u2.AddCell(p.Name, cmd, new UIClick(new WebMeta().Put(d, p.Name)).Send(request.Model, request.Command));
                }
                if (cfg2.Count == 0)
                {
                    u2.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有云模块").Put("icon", "\uF016"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),

                    new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));

                }
                ui.UIFootBar = new UIFootBar() { IsFixed = true };
                ui.UIFootBar.AddText(new UIEventText("配置云模块").Click(new UIClick("New").Send(request.Model, request.Command)),
                    new UIEventText("授权信息").Click(new UIClick().Send(request.Model, "License")).Style(new UIStyle().BgColor()));
                response.Redirect(ui);
                return this.DialogValue("New");
            });
            switch (svalue)
            {
                case "MAPPING":
                    {
                        var cfg2 = Reflection.Configuration("UMC");
                        var data3 = new System.Data.DataTable();
                        data3.Columns.Add("model");
                        data3.Columns.Add("cmd");
                        data3.Columns.Add("text");
                        data3.Columns.Add("root");


                        for (var i = 0; i < cfg2.Count; i++)
                        {
                            var p = cfg2[i];
                            var cmd = p.Type;

                            if (String.IsNullOrEmpty(p.Type))
                            {
                                cmd = "*";
                            }

                            data3.Rows.Add(p.Name, cmd, p["desc"], p["root"]);

                        }

                        this.Context.Send($"{request.Model}.{request.Command}", new WebMeta().Put("data", data3), true);
                    }
                    break;
            }

            var cfg = Reflection.Configuration("UMC");
            var n = cfg[svalue] ?? Data.Provider.Create(String.Empty, String.Empty);
            var Settings = this.AsyncDialog("Settings", g =>
            {
                var fm = new UIFormDialog() { Title = "配置云模块" };

                if (String.IsNullOrEmpty(n.Name))
                    fm.AddText("云模块名", "name", n.Name);
                else
                {
                    fm.AddTextValue().Put("云模块名", n.Name);
                }
                fm.AddText("指令通配符", "type", n.Type);
                fm.AddText("描述", "desc", n["desc"]);
                fm.AddText("服务站点", "root", n["root"]);
                if (String.IsNullOrEmpty(n.Name) == false)
                {
                    fm.AddCheckBox("", "Status", "NO").Put("移除", "DEL");
                }
                fm.Submit("确认", $"{request.Model}.{request.Command}");
                return fm;
            });
            var status = Settings["Status"] ?? "";
            if (status.Contains("DEL"))
            {
                cfg.Remove(svalue);
            }
            else
            {
                var p = Data.Provider.Create(Settings["name"] ?? svalue, Settings["type"]);
                p.Attributes.Add("root", Settings["root"]);
                p.Attributes.Add("desc", Settings["desc"]);

                cfg.Add(p);
            }
          


            Reflection.Configuration("UMC", cfg);
            var data2 = new System.Data.DataTable();
            data2.Columns.Add("model");
            data2.Columns.Add("cmd");
            data2.Columns.Add("text");
            data2.Columns.Add("root");


            for (var i = 0; i < cfg.Count; i++)
            {
                var p = cfg[i];
                var cmd = p.Type;

                if (String.IsNullOrEmpty(p.Type))
                {
                    cmd = "*";
                }

                data2.Rows.Add(p.Name, cmd, p["desc"], p["root"]);

            }

            this.Context.Send($"{request.Model}.{request.Command}", new WebMeta().Put("data", data2), false);

            this.Prompt("配置成功");

        }
    }

}
