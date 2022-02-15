using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Net;

namespace UMC.Web.Activity
{

    class SystemLicenseActivity : WebActivity
    {
        static string GetSize(long b)
        {

            String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };
            long mod = 1000;
            int i = 0;
            while (b >= mod)
            {
                b /= mod;
                i++;
            }
            return b + units[i];

        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var appId = WebResource.Instance().Provider["appId"];
            if (String.Equals(request.SendValue, "Check"))
            {
                if (String.IsNullOrEmpty(appId))
                {
                    response.Redirect(request.Model, request.Command, new Web.UIConfirmDialog("当前应用未注册，请完成登记注册")
                    {
                        Title = "应用注册",
                        DefaultValue = "Select"
                    });
                }
                return;
            }

            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能管理注册信息");
            }

            var secret = WebResource.Instance().Provider["appSecret"];

            var model = Web.UIDialog.AsyncDialog("Model", d =>
            {

                if (String.IsNullOrEmpty(appId))
                {
                    return this.DialogValue("Select");

                }
                var form = request.SendValues ?? new WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command)
                        .RefreshEvent("License")
                        .Builder(), true);

                }

                if (String.IsNullOrEmpty(secret))
                {
                    this.Prompt("效验码不存在");
                }
                var webr4 = new Uri(String.Format("https://api.365lu.cn/{0}/Transfer", Utility.Parse36Encode(Utility.Guid(appId).Value))).WebRequest();
                HotCache.Sign(webr4, new System.Collections.Specialized.NameValueCollection(), secret);

                var ui = UISection.Create(new UITitle("授权信息"));
                var meta = JSON.Deserialize<WebMeta>(webr4.Get().ReadAsString());


                ui.AddCell("联系人", meta["contact"], new UIClick(new WebMeta(d, "Contact")).Send(request.Model, request.Command))
                .AddCell("联系电话", meta["tel"], new UIClick(new WebMeta(d, "Tel")).Send(request.Model, request.Command))
                .AddCell("电子邮件", meta["email"], new UIClick(new WebMeta(d, "Email")).Send(request.Model, request.Command));

                ui.NewSection().AddCell("主体名称", meta["caption"], new UIClick(new WebMeta(d, "Name")).Send(request.Model, request.Command))
                .AddCell("主体所在地", meta["address"], new UIClick(new WebMeta(d, "Address")).Send(request.Model, request.Command));

                var skey = meta["key"];
                var domain = meta["domain"];

                ui.NewSection().AddCell("穿透域名", meta["domain"], new UIClick(new WebMeta(d, "Domain")).Send(request.Model, request.Command));

                ui.NewSection().AddCell("剩余流量", GetSize(Convert.ToInt64(meta["allowSize"])))
                .AddCell("上行流量", GetSize(Convert.ToInt64(meta["inputSize"])))
                .AddCell("下行流量", GetSize(Convert.ToInt64(meta["outputSize"])));

                ui.NewSection().AddCell("授权版本", meta.ContainsKey("enterprise") ? "企业版" : "个人版");



                ui.UIFootBar = new UIFootBar() { IsFixed = true };
                ui.UIFootBar.AddText(new UIEventText("重置授权码").Click(new UIClick(new WebMeta(d, "License")).Send(request.Model, request.Command)),
                    new UIEventText("流量充值").Click(new UIClick(new WebMeta(d, "Recharge")).Send(request.Model, request.Command)).Style(new UIStyle().BgColor()));
                response.Redirect(ui);

                return this.DialogValue("none");
            });
            switch (model)
            {
                case "Select":
                    var ls = new UISheetDialog() { Title = "注册登记" };
                    ls.Options.Add(new UIClick("Register") { Text = "我没有授权码" }.Send(request.Model, request.Command));
                    ls.Options.Add(new UIClick("License") { Text = "我已有授权码" }.Send(request.Model, request.Command));
                    response.Redirect(request.Model, request.Command, ls);

                    break;
                case "Register":
                    {

                        var register = this.AsyncDialog(g =>
                        {
                            var fm = new UMC.Web.UIFormDialog() { Title = "登记注册" };
                            fm.AddText("联系人", "Contact", String.Empty).Put("placeholder", "联系人真实人名");
                            fm.AddPhone("联系电话", "Tel", String.Empty);
                            fm.AddText("电子邮件", "Email", String.Empty).Put("placeholder", "建议联系人QQ邮箱");
                            fm.AddText("主体名称", "Name", String.Empty).PlaceHolder("所属公司或门店名称");

                            fm.AddOption("主体所在地", "Address", String.Empty, String.Empty).PlaceHolder("公司或门店地址")
                            .Command("Settings", "Area");
                            fm.Submit("确认注册", request, "License");
                            return fm;
                        }, "Setings");
                        var rwebr = new Uri("https://api.365lu.cn/Register").WebRequest();
                        rwebr.Headers.Add("umc-client-pfm", "sync");
                        var resp = rwebr.Post(register);
                        if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var meta = JSON.Deserialize<WebMeta>(resp.ReadAsString());
                            if (meta.ContainsKey("msg"))
                            {
                                this.Prompt(meta["msg"]);
                            }
                            if (meta.ContainsKey("success"))
                            {
                                var provider = UMC.Data.WebResource.Instance().Provider;
                                var pc = Reflection.Configuration("assembly") ?? new ProviderConfiguration();
                                pc.Providers["WebResource"] = provider;
                                provider.Attributes["host"] = meta["domain"];
                                provider.Attributes["appSecret"] = meta["appSecret"];
                                provider.Attributes["appId"] = meta["appId"];
                                var union = meta["union"] as string;
                                if (String.IsNullOrEmpty(union) == false)
                                {
                                    provider.Attributes["union"] = union;
                                }
                                pc.WriteTo(UMC.Data.Reflection.AppDataPath("UMC//assembly.xml"));
                                this.Prompt("授权成功", "登记成功，个人版授权已成功，权益请参考个人版说明", false);
                                this.Context.Send("License", true);
                            }
                        }
                        else
                        {
                            this.Prompt("网络异常，请确认网络连接");
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
                            fm.Submit("确认授权", request, "License");
                            return fm;
                        });
                        var appId2 = Utility.Guid(Config["appId"]);
                        if (appId2.HasValue == false)
                        {
                            this.Prompt("不存在此标识码格式");
                        }
                        var webR = new Uri(String.Format("https://api.365lu.cn/{0}/Transfer/", Utility.Parse36Encode(appId2.Value))).WebRequest();

                        HotCache.Sign(webR, new System.Collections.Specialized.NameValueCollection(), Config["appSecret"]);

                        var resp = webR.Get();
                        if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var con = UMC.Data.JSON.Deserialize(resp.ReadAsString()) as Hashtable;
                            var pc = Reflection.Configuration("assembly") ?? new ProviderConfiguration();
                            pc.Providers["WebResource"] = UMC.Data.WebResource.Instance().Provider;
                            provider.Attributes["host"] = con["domain"] as string;
                            provider.Attributes["appSecret"] = Config["appSecret"];
                            provider.Attributes["appId"] = con["appId"] as string;
                            var union = con["union"] as string;
                            if (String.IsNullOrEmpty(union) == false)
                            {
                                provider.Attributes["union"] = union;
                            }

                            pc.WriteTo(UMC.Data.Reflection.AppDataPath("UMC//assembly.xml"));
                            this.Context.Send("License", true);
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
            var webr = new Uri(String.Format("https://api.365lu.cn/{0}/Transfer", Utility.Parse36Encode(Utility.Guid(appId).Value))).WebRequest();

            HotCache.Sign(webr, new System.Collections.Specialized.NameValueCollection(), secret);

            var applyS = this.AsyncDialog(g =>
            {
                var meta = JSON.Deserialize<WebMeta>(webr.Get().ReadAsString());
                var caption = meta["contact"];
                var fm = new UMC.Web.UIFormDialog() { Title = "授权信息" };
                switch (model)
                {
                    case "Contact":
                        fm.AddText("联系人", "Contact", meta["contact"]).Put("placeholder", "联系人真实人名");
                        break;
                    case "Tel":
                        fm.AddPhone("联系电话", "Tel", meta["tel"]);
                        break;
                    case "Email":
                        fm.AddText("电子邮件", "Email", meta["email"]).Put("placeholder", "建议联系人QQ邮箱");
                        break;
                    case "Name":
                        fm.AddText("主体名称", "Name", meta["caption"]).PlaceHolder("所属公司或门店名称");
                        break;
                    case "Address":
                        fm.AddOption("主体所在地", "Address", meta["address"], meta["address"]).PlaceHolder("公司或门店地址")
                        .Command("Settings", "Area");
                        break;
                    case "Domain":
                        fm.AddText("穿透域名", "Domain", String.Empty).PlaceHolder("只支持 0-9a-z");
                        break;
                    case "Recharge":
                        fm.Title = "流量充值";
                        if (meta.ContainsKey("recharge"))
                        {
                            fm.AddImage(new Uri(meta["recharge"]));
                            fm.AddPrompt(meta["rechargeText"] ?? "请用支付宝或微信扫一扫，完成充值");
                            fm.Submit("关闭");//, request, "License");
                            return fm;
                        }
                        else
                        {
                            this.Prompt("充值提示", "目前不支持在线充值，请联系渠道经销商");
                        }
                        break;
                    case "Version":
                        this.Prompt("授权版本", "版本差异，请在“云桌面--帮助文档--版本差异”中查看。");
                        break;
                    default:
                        this.Prompt("不支持此设置");
                        break;

                }

                fm.Submit("确认提交", request, "License");
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
                case "Domain":
                    {
                        var pDomain = applyS["Domain"];
                        if (System.Text.RegularExpressions.Regex.IsMatch(pDomain, "^[\\da-z]+$") == false)
                        {
                            this.Prompt("穿透域名只支持数字和小写字母");

                        }
                        var webr2 = new Uri(HotCache.Uri, $"Transfer?Domain={pDomain}").WebRequest();

                        var ns2 = new System.Collections.Specialized.NameValueCollection();

                        HotCache.Sign(webr2, ns2, secret);
                        var meta = JSON.Deserialize<WebMeta>(webr.Get().ReadAsString());
                        if (meta.ContainsKey("msg"))
                        {
                            this.Prompt(meta["msg"]);
                        }
                        var provider = Data.WebResource.Instance().Provider;

                        var pc = Reflection.Configuration("assembly") ?? new ProviderConfiguration(); ;
                        provider.Attributes["host"] = meta["domain"];
                        var union = meta["union"] as string;
                        if (String.IsNullOrEmpty(union) == false)
                        {
                            provider.Attributes["union"] = union;
                        }
                        pc.Providers["WebResource"] = provider;
                        UMC.Data.DataFactory.Instance().Configuration("assembly", pc);
                        this.Context.Send("License", true);
                    }

                    break;
            }



        }

    }
}
