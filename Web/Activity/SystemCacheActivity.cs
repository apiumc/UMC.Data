using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Data;
using UMC.Data.Sql;
using UMC.Web;
using UMC.Net;
using System.Linq;

namespace UMC.Web.Activity
{
    class SystemCacheActivity : UMC.Web.WebActivity
    {
        public override void ProcessActivity(UMC.Web.WebRequest request, UMC.Web.WebResponse response)
        {
            if (request.IsMaster == false)
            {
                this.Prompt("高速存储，需要管理员权限才能查看");
            }
            var Key = this.AsyncDialog("Key", g =>
            {
                var form = request.SendValues ?? new UMC.Web.WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta(request.Arguments))
                        .RefreshEvent("Cache.Event")
                            .Builder(), true);
                }
                var uTitle = new UITitle("高速存储");

                var ui = UISection.Create(uTitle);

                var data = UMC.Data.HotCache.Caches().OrderBy(r => r.Name).GetEnumerator();

                while (data.MoveNext())
                {
                    var cacah = data.Current;
                    ui.AddCell(cacah.Name, String.Format("{0}条数", cacah.Count), new UIClick(g, "Import", "NameCode", cacah.NameCode.ToString()).Send(request.Model, request.Command));
                }


                var Subscribes = NetSubscribe.Subscribes;
                if (Subscribes.Length > 0)
                {
                    for (var i = 0; i < Subscribes.Length; i++)
                    {
                        var s = Subscribes[i];
                        var sui = ui.NewSection();
                        sui.Delete(UICell.UI("订阅节点", s.Address), new UIEventText("移除").Click(new UIClick(g, "SubscribeDel", "Subscribe", s.Address).Send(request.Model, request.Command)));

                        sui.AddCell("服务状态", s.IsBridging ? "已连接" : "未连接", new UIClick(g, "Subscribe", "Subscribe", s.Address).Send(request.Model, request.Command));

                    }
                }
                else
                {
                    var hot = ui.NewSection();
                    hot.Header.Put("text", "订阅节点");
                    hot.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有订阅节点").Put("icon", "\uEA05")
                , new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 


                }
                var Publishes = NetSubscribe.Publishes;

                if (Publishes.Length > 0)
                {
                    for (var i = 0; i < Publishes.Length; i++)
                    {
                        var s = Publishes[i];
                        var sui = ui.NewSection();
                        sui.Delete(UICell.UI("发布节点", s.Address), new UIEventText("移除").Click(new UIClick(g, "PublisherDel", "Subscribe", s.Key).Send(request.Model, request.Command)));

                        sui.AddCell("服务状态", s.IsBridging ? "已连接" : "未连接", new UIClick(g, "Publisher", "Subscribe", s.Key).Send(request.Model, request.Command));
                    }
                }
                else
                {
                    var hot = ui.NewSection();
                    hot.Header.Put("text", "发布节点");
                    hot.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有发布节点").Put("icon", "\uEA05")
                , new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 


                }
                ui.UIFootBar = new UIFootBar() { IsFixed = true };
                ui.UIFootBar.AddText(new UIEventText("新增订阅").Click(new UIClick(new WebMeta(g, "Puls")).Send(request.Model, request.Command)),
                    new UIEventText("收缩保存").Click(new UIClick(new WebMeta(g, "Save")).Send(request.Model, request.Command)).Style(new UIStyle().BgColor()));

                response.Redirect(ui);
                return this.DialogValue("none");
            });
            switch (Key)
            {
                case "Puls":
                    var urlStr = this.AsyncDialog("Url", r =>
                    {
                        return new UITextDialog() { Title = "新增订阅节点" };
                    });

                    var url = new Uri(new Uri(urlStr), "/UMC.Synch");

                    var p = WebResource.Instance();
                    String appId = p.Provider["appId"];
                    var secret = p.Provider["appSecret"];

                    if (String.IsNullOrEmpty(secret) || String.IsNullOrEmpty(appId))
                    {
                        this.Prompt("请先完成注册,再来订阅节点");
                    }

                    var time = UMC.Data.Utility.TimeSpan().ToString();
                    var nvs = new System.Collections.Specialized.NameValueCollection();

                    nvs.Add("from", appId);
                    nvs.Add("time", time);
                    nvs.Add("point", request.Server);

                    var webD = new Web.WebMeta();
                    webD.Put("from", appId);
                    webD.Put("time", time);
                    webD.Put("point", request.Server);
                    webD.Put("sign", UMC.Data.Utility.Sign(nvs, secret));

                    var content = new System.Net.Http.StringContent(UMC.Data.JSON.Serialize(webD, "ts"), System.Text.Encoding.UTF8, "application/json");


                    var xhr = url.WebRequest().Post(content);

                    var hsh = JSON.Deserialize<Hashtable>(xhr.ReadAsString());

                    if (hsh == null)
                    {
                        this.Prompt("验证不通过");
                    }
                    else if (hsh.ContainsKey("verify"))
                    {

                        var subscribe = NetSubscribe.Subscribes.FirstOrDefault(r => r.Address == url.Host);
                        if (subscribe != null)
                        {
                            this.Prompt("此订阅节点已经存在");
                        }
                        else
                        {
                            new NetSubscribe(request.Server, url.Host, url.Port, WebResource.Instance().Provider["appSecret"]);
                        }
                        var hask = UMC.Data.Provider.Create(url.Host, "Subscribe");
                        hask.Attributes["url"] = new Uri(url, "/").AbsoluteUri;
                        var Subscribe = ProviderConfiguration.GetProvider(Reflection.AppDataPath("UMC//Subscribe.xml")) ?? new ProviderConfiguration();
                        Subscribe.Add(hask);
                        Subscribe.WriteTo(Reflection.AppDataPath("UMC//Subscribe.xml"));

                    }
                    else
                    {
                        this.Prompt((hsh["msg"] as string) ?? "验证不通过");
                    }
                    this.Context.Send("Cache.Event", true);
                    break;
                case "Subscribe":
                    {
                        var key = this.AsyncDialog("Subscribe", "none");
                        var subscribe = NetSubscribe.Subscribes.FirstOrDefault(r => r.Address == key);
                        if (subscribe != null)
                        {
                            if (subscribe.IsBridging)
                            {
                                subscribe.Write(0, new byte[0], 0, 0);
                            }
                            else
                            {
                                this.Prompt("订阅节点不在线", false);
                            }
                            this.Context.Send("Cache.Event", true);
                        }
                        else
                        {
                            this.Prompt("此订阅节点不存在");
                        }
                    }
                    break;
                case "SubscribeDel":
                    {
                        var key = this.AsyncDialog("Subscribe", "none");
                        var subscribe = NetSubscribe.Subscribes.FirstOrDefault(r => r.Address == key);
                        if (subscribe != null)
                        {

                            subscribe.Remove();

                            this.Context.Send("Cache.Event", true);
                        }
                        else
                        {
                            this.Prompt("此订阅节点不存在");
                        }
                    }
                    break;
                case "Publisher":
                    {
                        var key = this.AsyncDialog("Subscribe", "none");
                        var subscribe = NetSubscribe.Publishes.FirstOrDefault(r => r.Key == key);
                        if (subscribe != null)
                        {
                            if (subscribe.IsBridging)
                            {
                                subscribe.Write(0, new byte[0], 0, 0);
                            }
                            else
                            {
                                this.Prompt("订阅节点不在线", false);
                            }
                            this.Context.Send("Cache.Event", true);
                        }
                        else
                        {
                            this.Prompt("此订阅节点不存在");
                        }
                    }
                    break;
                case "PublisherDel":
                    {
                        var key = this.AsyncDialog("Subscribe", "none");
                        var subscribe = NetSubscribe.Publishes.FirstOrDefault(r => r.Key == key);
                        if (subscribe != null)
                        {

                            subscribe.Remove();


                            this.Context.Send("Cache.Event", true);
                        }
                        else
                        {
                            this.Prompt("此订阅节点不存在");
                        }
                    }
                    break;
                case "Save":
                    HotCache.Save();
                    break;
                case "Import":
                    {
                        var nameCode = UMC.Data.Utility.IntParse(this.AsyncDialog("NameCode", "0"), 0);
                        var key = UIDialog.AsyncDialog(this.Context, "Subscribe", r =>
                        {
                            var hs = HotCache.Caches().FirstOrDefault(c => c.NameCode == nameCode);
                            var form = new UMC.Web.UIFormDialog() { Title = "数据合并" };
                            form.AddTextValue().Put("合并表名", hs.Name)
                            .Put("现有条数", hs.Count.ToString() + "条");
                            var rd = form.AddRadio("来源节点", "Subscribe");

                            var Subscribes = NetSubscribe.Subscribes;
                            foreach (var s in Subscribes)
                            {
                                rd.Add(s.Address);
                            }
                            form.Submit("确认合并", "Cache.Event");
                            return form;
                        });
                        var subscribe = NetSubscribe.Subscribes.FirstOrDefault(r => r.Address == key);
                        if (subscribe != null)
                        {
                            if (subscribe.IsBridging)
                            {
                                subscribe.Import(nameCode);

                            }
                            else
                            {
                                this.Prompt("订阅节点不在线", false);
                            }
                            this.Context.Send("Cache.Event", true);
                        }
                        else
                        {
                            this.Prompt("此订阅节点不存在");
                        }
                    }
                    break;
                default:
                    this.Context.Send("Cache.Event", true);
                    break;
            }


        }


    }
}