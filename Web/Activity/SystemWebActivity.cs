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
                        .RefreshEvent("Config")
                        .Builder(), true);

                }
                var title = UITitle.Create();

                title.Title = "应用安全";

                var ui = UISection.Create(title);
                var webr = UMC.Data.WebResource.Instance();
                var appKey = UMC.Security.Principal.Current.AppKey ?? Guid.Empty;
                if (appKey == Guid.Empty)
                {
                    var appID = webr.Provider["appId"];
                    var appSecret = webr.Provider["appSecret"];
                    ui.AddCell("授权码", String.IsNullOrEmpty(appID) ? "去授权" : "去查看", new UIClick().Send(request.Model, "License")); 
                    ui.AddCell("检验码", String.IsNullOrEmpty(appSecret) ? "未设置" : "已设置", new UIClick().Send(request.Model, "License"));

                    ui.NewSection().AddCell('\uea04', "安全码", "用于应用交互效验", new UIClick(new WebMeta().Put(d, "APPSECRET")).Send(request.Model, request.Command));
                }
                else
                {
                    ui.AddCell("授权码", UMC.Data.Utility.Guid(appKey))
                    .AddCell('\uea04', "安全码", "用于应用交互效验", new UIClick(new WebMeta().Put(d, "APPSECRET")).Send(request.Model, request.Command));
                }


                var cfg2 = DataFactory.Instance().Configuration("UMC") ?? new ProviderConfiguration();

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
                    new UIEventText("重置安全码").Click(new UIClick("RESET").Send(request.Model, request.Command)).Style(new UIStyle().BgColor()));
                response.Redirect(ui);
                return this.DialogValue("New");
            });
            switch (svalue)
            {
                case "Command":
                    var Command = this.AsyncDialog("Command", g =>
                    {
                        var fm = new UIFormDialog() { Title = "触发指令" };

                        fm.AddText("模块", "Model", "").PlaceHolder("触发的模块");
                        fm.AddText("指令", "Command", "").PlaceHolder("触发的指令");
                        fm.AddText("参数", "Send", "").NotRequired().PlaceHolder("触发的参数");
                        return fm;
                    });
                    var send = Command["Send"];
                    if (String.IsNullOrEmpty(send) == false)
                    {

                        response.Redirect(Command["Model"], Command["Command"], Command["Send"]);
                    }
                    else
                    {

                        response.Redirect(Command["Model"], Command["Command"]);
                    }
                    break;
                case "RESETAPPSECRET":
                    this.Context.Send("Config", false);
                    this.Prompt("重置安全码", "AppSecret：" + Data.WebResource.Instance().AppSecret(true));

                    break;
                case "RESET":
                    response.Redirect(request.Model, request.Command, new UMC.Web.UIConfirmDialog("重置安全码后，老的安全码将会过期，您确认重置吗?") { DefaultValue = "RESETAPPSECRET" });
                    break;
                case "APPSECRET":
                    this.Prompt("安全码", "AppSecret：" + Data.WebResource.Instance().AppSecret());
                    break;
                case "SCANNING":
                    Reflection.Instance().ScanningClass();
                    Utility.Writer(Utility.MapPath("App_Data/register.net"), JSON.Serialize(new WebMeta().Put("time", UMC.Data.Utility.TimeSpan()).Put("data", WebRuntime.RegisterCls())), false);
                    this.Prompt("已从新扫描类型");
                    break;
            }

            var cfg = DataFactory.Instance().Configuration("UMC") ?? new ProviderConfiguration();
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
                fm.AddText("服务网址", "src", n["src"]);
                fm.AddText("效验码", "secret", n["secret"]).NotRequired();
                if (String.IsNullOrEmpty(n.Name) == false)
                {
                    fm.AddCheckBox("", "Status", "NO").Put("移除", "DEL");
                }
                fm.Submit("确认", request, "Config");
                return fm;
            });
            var status = Settings["Status"] ?? "";
            if (status.Contains("DEL"))
            {
                cfg.Providers.Remove(svalue);
            }
            else
            {
                var src = new Uri(Settings["src"]);
                var p = Data.Provider.Create(Settings["name"] ?? svalue, Settings["type"]);
                p.Attributes.Add("src", src.AbsoluteUri);
                p.Attributes.Add("desc", Settings["desc"]);
                if (String.IsNullOrEmpty(Settings["secret"]) == false)
                    p.Attributes.Add("secret", Settings["secret"]);
                cfg.Providers[p.Name] = p;//.Add(p.Name, p);
            }
            UMC.Data.ProviderConfiguration.Cache.Clear();


            DataFactory.Instance().Configuration("UMC", cfg);
            var data2 = new System.Data.DataTable();
            data2.Columns.Add("model");
            data2.Columns.Add("cmd");
            data2.Columns.Add("text");
            data2.Columns.Add("src");


            for (var i = 0; i < cfg.Count; i++)
            {
                var p = cfg[i];
                var cmd = p.Type;

                if (String.IsNullOrEmpty(p.Type))
                {
                    cmd = "*";
                }

                data2.Rows.Add(p.Name, cmd, p["desc"], p["src"]);

            }

            this.Context.Send("Config", new WebMeta().Put("data", data2), false);

            this.Prompt("配置成功");

        }
    }

}
