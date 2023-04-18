using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UMC.Data;
using UMC.Data.Sql;
using UMC.Net;

namespace UMC.Web.Activity
{

    class SystemLicenseActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理注册信息");
            }

            var appId = WebResource.Instance().Provider["appId"];
            var secret = WebResource.Instance().Provider["appSecret"];

            var model = Web.UIDialog.AsyncDialog(this.Context, "Model", d =>
             {

                 if (String.IsNullOrEmpty(appId))
                 {
                     return this.DialogValue("Select");

                 }
                 if (String.IsNullOrEmpty(secret))
                 {
                     this.Prompt("效验码不存在");
                 }
                 var form = request.SendValues ?? new WebMeta();
                 if (form.ContainsKey("limit") == false)
                 {
                     this.Context.Send(new UISectionBuilder(request.Model, request.Command)
                         .RefreshEvent("License").CloseEvent("noLicense")
                         .Builder(), true);

                 }

                 var webr2 = new Uri(APIProxy.Uri, "Transfer").WebRequest();
                 APIProxy.Sign(webr2, new System.Collections.Specialized.NameValueCollection(), secret);

                 var ui = UISection.Create(new UITitle("授权信息"));
                 var xhr = webr2.Get();

                 switch (xhr.StatusCode)
                 {
                     case System.Net.HttpStatusCode.Unauthorized:
                     case System.Net.HttpStatusCode.Forbidden:
                         this.Context.Send("noLicense", false);
                         response.Redirect(request.Model, request.Command, new UIConfirmDialog("检验不通过或注册信息有误,请从新注册") { DefaultValue = "Select" });

                         break;
                 }
                 var meta = JSON.Deserialize<WebMeta>(xhr.ReadAsString()) ?? new WebMeta();


                 ui.AddCell("主体名称", meta["caption"], new UIClick(new WebMeta(d, "Name")).Send(request.Model, request.Command))
                  .AddCell("主体所在地", meta["address"], new UIClick(new WebMeta(d, "Address")).Send(request.Model, request.Command));


                 ui.NewSection().AddCell("联系人", meta["contact"], new UIClick(new WebMeta(d, "Name")).Send(request.Model, request.Command))
                 .AddCell("联系电话", meta["tel"], new UIClick(new WebMeta(d, "Name")).Send(request.Model, request.Command))
                 .AddCell("电子邮件", meta["email"], new UIClick(new WebMeta(d, "Email")).Send(request.Model, request.Command));

                 var skey = meta["key"];
                 var domain = meta["domain"];



                 //ui.NewSection().AddCell("VPN域名", meta["domain"]).AddCell("VPN流量", meta["allowSize"])
                 //   .AddCell("流量过期", meta["expireTime"]);
                 var p = Assembly.GetEntryAssembly().GetCustomAttributes().First(r => r is System.Reflection.AssemblyInformationalVersionAttribute) as System.Reflection.AssemblyInformationalVersionAttribute;


                 var now = Utility.TimeSpan();


                 var vL = 0;
                 var lui = ui.NewSection();
                 var IsHave = false;

                 var apps = meta.GetDictionary()["apps"] as Array;
                 var cl = apps?.Length;
                 for (var i = 0; i < cl; i++)
                 {
                     var hash = apps.GetValue(i) as System.Collections.Hashtable;
                     var ExpireTime = Utility.IntParse(hash["Expire"].ToString(), 0);
                     var Quantity = Utility.IntParse(hash["Quantity"].ToString(), 0);
                     if (Quantity > 0)
                     {

                         if (ExpireTime > now)
                         {
                             lui.AddCell('\uf09c', $"包含{Quantity}个应用许可证", $"还剩{new TimeSpan(0, 0, ExpireTime - now).TotalDays:0.00}天");
                             vL += Quantity;
                             IsHave = true;
                         }
                         else if (ExpireTime < 10)
                         {
                             lui.AddCell('\uf23e', $"{ExpireTime}年期预许可", $"{Quantity}个");
                             IsHave = true;

                         }

                     }
                 }

                 if (IsHave)
                 {
                     lui.AddCell('\uf084', "有效应用数", $"{vL}个").NewSection().AddCell("运行版本", p.InformationalVersion);

                 }
                 else
                 {
                     lui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "还未有应用许可,请去获得许可").Put("icon", "\uea05"), new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"),
                    new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));
                     // ui.NewSection().AddCell("运行版本", p.InformationalVersion);

                 }
                 ui.NewSection().AddCell("运行版本", p.InformationalVersion)
                 .NewSection().AddCell("联系官方", "天才工程师为你解答", new UIClick("Contact").Send(request.Model, request.Command));




                 ui.UIFootBar = new UIFootBar() { IsFixed = true };
                 ui.UIFootBar.AddText(new UIEventText("重置授权码").Click(new UIClick(new WebMeta(d, "Select")).Send(request.Model, request.Command)),
                     new UIEventText("去获得许可").Click(new UIClick(new WebMeta(d, "Recharge")).Send(request.Model, request.Command)).Style(new UIStyle().BgColor()));
                 response.Redirect(ui);

                 return this.DialogValue("none");
             });
            switch (model)
            {
                case "Select":
                    var ls = new UISheetDialog() { Title = "注册登记" }
                    .Put("我没有授权码", "Register")
                    .Put("我已有授权码", "License");
                    response.Redirect(request.Model, request.Command, ls);

                    break;
                case "Register":
                    {
                        var token = UMC.Data.Utility.Guid(this.Context.Token.Device.Value);
                        var rwebr = new Uri(APIProxy.Uri, "/Register").WebRequest();
                        rwebr.Headers.Add("umc-client-pfm", "sync");
                        var p = Assembly.GetEntryAssembly().GetCustomAttributes().First(r => r is System.Reflection.AssemblyInformationalVersionAttribute) as System.Reflection.AssemblyInformationalVersionAttribute;

                        rwebr.Headers.Add("umc-app-version", p.InformationalVersion);
                        var resp = rwebr.Post(new WebMeta().Put("token", token));

                        if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var meta = JSON.Deserialize<WebMeta>(resp.ReadAsString());

                            if (meta.ContainsKey("success"))
                            {
                                var provider = UMC.Data.WebResource.Instance().Provider;
                                var pc = Reflection.Configuration("assembly") ?? new ProviderConfiguration();
                                pc.Add(provider);
                                provider.Attributes["domain"] = meta["domain"];
                                provider.Attributes["appSecret"] = meta["appSecret"];
                                provider.Attributes["appId"] = meta["appId"];
                                var union = meta["union"] as string;
                                if (String.IsNullOrEmpty(union) == false)
                                {
                                    provider.Attributes["union"] = union;
                                }
                                Reflection.Configuration("assembly", pc);
                                this.Prompt("注册登记成功", false);
                                this.Context.Send("License", false);

                                this.Context.Send(new UISectionBuilder(request.Model, request.Command)
                                    .RefreshEvent("License")
                                    .Builder(), true);

                            }
                            else if (meta.ContainsKey("url"))
                            {
                                var Config = this.AsyncDialog("Config", g =>
                                {
                                    var fm = new UIFormDialog() { Title = "应用登记" };
                                    fm.AddImage(new Uri(UMC.Data.Utility.QRUrl(meta["url"])));

                                    fm.AddConfirm(meta["desc"] ?? "请用微信扫一扫上图二维码，完成登记授权", "token", token);

                                    fm.Submit("下一步", "License");
                                    fm.AddPrompt(meta["tip"] ?? "当在微信中完成登记时，请点击“下一步”，完成登记信息同步");
                                    return fm;
                                });
                                if (meta.ContainsKey("msg"))
                                {
                                    this.Prompt(meta["msg"]);
                                }

                            }
                            else if (meta.ContainsKey("msg"))
                            {
                                this.Prompt(meta["msg"]);
                            }
                        }
                        else
                        {
                            this.Prompt("注册时通信异常");
                        }
                    }
                    break;
                case "Contact":
                    {
                        var rwebr = new Uri(APIProxy.Uri, "/Register").WebRequest();
                        rwebr.Headers.Add("umc-client-pfm", "sync");
                        var p = Assembly.GetEntryAssembly().GetCustomAttributes().First(r => r is System.Reflection.AssemblyInformationalVersionAttribute) as System.Reflection.AssemblyInformationalVersionAttribute;

                        rwebr.Headers.Add("umc-app-version", p.InformationalVersion);
                        var xhr = rwebr.Post(new WebMeta().Put("Contact", true));

                        switch (xhr.StatusCode)
                        {
                            case System.Net.HttpStatusCode.OK:

                                var data = JSON.Deserialize<WebMeta>(xhr.ReadAsString()) ?? new WebMeta();

                                var Config = this.AsyncDialog("Contact", g =>
                                {
                                    var fm = new UIFormDialog() { Title = "联系官方" };

                                    var dd = data["DD"];

                                    fm.AddImage(new Uri(data["WeiXin"]));
                                    //fm.AddPrompt("用微信扫一扫");

                                    fm.Submit("用微信扫一扫");
                                    fm.Add(UICell.UI("用钉钉联系", "", new UIClick($"dingtalk://dingtalkclient/action/sendmsg?dingtalk_id={dd}") { Key = "Url" }));


                                    return fm;
                                });
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case "License":
                    {
                        var provider = UMC.Data.WebResource.Instance().Provider;
                        var Config = this.AsyncDialog("Config", g =>
                        {
                            var fm = new UIFormDialog() { Title = "应用授权" };
                            fm.AddText("授权码", "appId", String.Empty);
                            fm.AddText("检验码", "appSecret", "");

                            fm.Submit("确认授权", "License");
                            return fm;
                        });
                        var appId2 = Utility.Guid(Config["appId"]);
                        if (appId2.HasValue == false)
                        {
                            this.Prompt("不存在此标识码格式");
                        }
                        var webR = new Uri(APIProxy.Uri, $"/{Utility.Parse36Encode(appId2.Value)}/Transfer/").WebRequest();

                        APIProxy.Sign(webR, new System.Collections.Specialized.NameValueCollection(), Config["appSecret"]);

                        var resp = webR.Get();
                        if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var con = UMC.Data.JSON.Deserialize(resp.ReadAsString()) as Hashtable;
                            var pc = Reflection.Configuration("assembly") ?? new ProviderConfiguration();
                            pc.Add(provider);

                            provider.Attributes["domain"] = con["domain"] as string;
                            provider.Attributes["appSecret"] = Config["appSecret"];
                            provider.Attributes["appId"] = con["appId"] as string;
                            var union = con["union"] as string;
                            if (String.IsNullOrEmpty(union) == false)
                            {
                                provider.Attributes["union"] = union;
                            }
                            Reflection.Configuration("assembly", pc);
                            this.Prompt("注册登记成功", false);
                            this.Context.Send("License", false);

                            this.Context.Send(new UISectionBuilder(request.Model, request.Command)
                                .RefreshEvent("License")
                                .Builder(), true);
                        }
                        else
                        {
                            this.Prompt("效验不通过，请核查授权信息");
                        }

                    }
                    break;
            }

            if (String.IsNullOrEmpty(secret))
            {
                this.Prompt("效验码不存在");
            }
            var webr = new Uri(APIProxy.Uri, "Transfer").WebRequest();
            APIProxy.Sign(webr, new System.Collections.Specialized.NameValueCollection(), secret);

            var applyS = this.AsyncDialog(g =>
            {
                var meta = JSON.Deserialize<WebMeta>(webr.Get().ReadAsString());
                var caption = meta["contact"];
                var fm = new UMC.Web.UIFormDialog() { Title = "授权信息" };
                switch (model)
                {
                    default:
                    case "Name":
                        fm.Title = "主体登记";
                        fm.AddText("主体名称", "Name", meta["caption"]).PlaceHolder("所属公司或门店名称");
                        fm.AddText("联系人姓名", "Contact", meta["contact"]).Put("placeholder", "联系人真实人名");
                        fm.AddPhone("手机号码", "Tel", meta["tel"]).Put("placeholder", "真实的手机号码");
                        break;
                    case "Email":
                        fm.Title = "主体登记";
                        fm.AddText("电子邮件", "Email", meta["email"]).Put("placeholder", "建议联系人QQ邮箱");
                        break;
                    case "Address":
                        fm.Title = "主体登记";
                        fm.AddOption("主体所在地", "Address", meta["address"], meta["address"]).PlaceHolder("公司或门店地址")
                        .Command("Settings", "Area");
                        break;
                    case "Recharge":

                        var data = JSON.Deserialize<System.Collections.Hashtable>(webr.Post(new WebMeta().Put("type", "App")).ReadAsString());

                        request.Arguments["API"] = data["src"] as string;
                        var Combo = data["Combo"] as Array;

                        fm.Title = "获取许可";
                        var style = new UIStyle();
                        style.Name("icon").Color(0x09bb07).Size(84).Font("wdk");
                        style.Name("title").Color(0x333).Size(20);
                        style.BgColor(0xfafcff).Height(200).AlignCenter();
                        var desc = new UMC.Web.WebMeta().Put("title", "应用许可").Put("icon", "\uea04");
                        fm.Config.Put("Header", new UIHeader().Desc(desc, "{icon}\n{title}", style));
                        var qty = this.AsyncDialog("Qty", "1");
                        var f = fm.AddRadio("单应用套餐", "Combo");
                        var cl = Combo.Length;
                        for (var i = 0; i < cl; i++)
                        {
                            var hash = Combo.GetValue(i) as System.Collections.Hashtable;
                            f.Put(hash["Text"] as string, hash["Value"] as string, i == cl - 1);
                        }

                        fm.AddNumber("应用数量", "Quantity", qty).Put("Compare", "GreaterEqual", "For", qty);
                        fm.Config.Put("Action", true);

                        fm.Submit("确认去购买");
                        return fm;

                }

                fm.Submit("确认提交", "License");
                return fm;
            }, "Setings");
            switch (model)
            {
                case "Tel":
                case "Email":
                case "Contact":
                case "Name":
                case "Address":
                    {

                        var meta = JSON.Deserialize<WebMeta>(webr.Post(applyS).ReadAsString());
                        if (meta.ContainsKey("msg"))
                        {
                            this.Prompt(meta["msg"]);
                        }
                        if (meta.ContainsKey("success"))
                        {
                            this.Context.Send("License", true);
                        }
                    }
                    break;
                case "Recharge":

                    var src = this.AsyncDialog("API", r =>
                    {
                        var appId = WebResource.Instance().Provider["appId"];
                        return this.DialogValue($"https://api.apiumc.com/UMC/Platform/Alipay/App?AuthKey={appId}");

                    });
                    var ComboValue = applyS["Combo"];
                    var Quantity = applyS["Quantity"];
                    response.Redirect(new Uri($"{src}&Combo={ComboValue}&Quantity={Quantity}"));
                    break;
            }



        }

    }
}
