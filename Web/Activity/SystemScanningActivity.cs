using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Data.Entities;
using UMC.Web;

namespace UMC.Web.Activity
{
    [Mapping("System", "Scanning", Auth = WebAuthType.All, Desc = "移动扫描处理", Weight = 0)]
    public class SystemScanningActivity : WebActivity
    {
        protected virtual void Scanning(Uri url)
        {
            var domain = new Uri(url, Data.WebResource.Instance().WebDomain()).AbsoluteUri;
            if (domain.Contains(url.Host))
            {
                var paths = new List<String>();

                paths.AddRange(url.LocalPath.Trim('/').Split('/'));
                if (paths.Count == 0)
                {
                    return;

                }
                switch (paths[0])
                {
                    case "download":
                    case "app":
                        if (String.IsNullOrEmpty(url.Query))
                        {
                            this.OpenUrl(url);
                        }
                        var query = url.Query.Substring(1);
                        var user = UMC.Security.Identity.Current;
                        if (user.IsAuthenticated == false)
                        {
                            this.Context.Response.Redirect("Account", "Login");
                        }
                        var dever = Data.Utility.Guid(query);
                        if (dever.HasValue)
                        {
                            var webr = Data.WebResource.Instance();
                            this.AsyncDialog("Device", g =>
                            {
                                webr.Push(dever.Value, new WebMeta().Put("msg", "扫码已经成功").Put("src", webr.ImageResolve(user.Id.Value, "1", 4)));
                                var fm = new UIFormDialog();
                                fm.Title = "扫码登录";
                                var desc = new UI.UIDesc(new UMC.Web.WebMeta().Put("desc", "正在进行扫码登录").Put("icon", "\uF108"));
                                desc.Desc("{icon}\n{desc}");
                                desc.Style.Align(1)
                                    .Color(0xaaa).Padding(40, 20).BgColor(0xfff).Name("icon", new UIStyle().Font("wdk").Size(160));
                                fm.Add(desc);

                                fm.Submit("确认登录", this.Context.Request, "PC.Login");
                                return fm;
                            });
                            var sesion = UMC.Data.DataFactory.Instance().Session(UMC.Security.AccessToken.Token.ToString());

                            if (sesion != null)
                            {
                                sesion.SessionKey = query;
                                UMC.Data.DataFactory.Instance().Post(sesion);
                            }
                            webr.Push(dever.Value, new WebMeta().Put("msg", "OK").Put("type", "SignIn").Put("root", Data.Utility.GetRoot(this.Context.Request.Url)));
                            this.Context.Send("PC.Login", true);
                        }
                        else
                        {
                            this.OpenUrl(url);
                        }
                        break;
                    case "Click":
                    case "Pager":
                        var p = paths[0];
                        paths.RemoveAt(0);
                        var data = new UMC.Web.WebMeta();
                        if (String.IsNullOrEmpty(url.Query) == false)
                        {
                            var query2 = System.Web.HttpUtility.ParseQueryString(url.Query.Substring(1));
                            for (var i = 0; i < query2.Count; i++)
                            {
                                var qV = query2.Get(i);
                                var qK = query2.GetKey(i);
                                if (String.IsNullOrEmpty(qK) == false)
                                {
                                    if (String.IsNullOrEmpty(qV) == false)
                                    {
                                        data.Put(qK, qV);
                                    }
                                    else
                                    {
                                        data.Put("Id", qK);
                                    }
                                }
                            }
                        }

                        var model = "Corp";
                        var cmd = "Scanning";
                        if (paths.Count > 1)
                        {
                            model = paths[0];
                            cmd = paths[1];
                            paths.RemoveRange(0, 2);

                        }
                        var SValue = String.Empty;
                        if (paths.Count == 1)
                        {
                            SValue = paths[0];
                            data.Put("Id", paths[0]);
                        }
                        else
                        {
                            while (paths.Count > 0)
                            {
                                if (paths.Count > 1)
                                {
                                    data.Put(paths[0], paths[1]);
                                }
                                paths.RemoveRange(0, 2);
                            }

                        }
                        if (p == "Click")
                        {
                            if (data.Count > 0)
                            {
                                if (String.IsNullOrEmpty(SValue) || data.Count > 1)
                                {

                                    this.Context.Response.Redirect(model, cmd, data, true);
                                }
                                else
                                {
                                    this.Context.Response.Redirect(model, cmd, SValue, true);
                                }
                            }
                            else
                            {
                                this.Context.Response.Redirect(model, cmd, true);
                            }
                        }
                        else
                        {
                            if (data.Count > 0)
                            {
                                this.Context.Send("Pager", new UMC.Web.WebMeta().Put("model", model, "cmd", cmd).Put("search", data), true);
                            }
                            else
                            {
                                this.Context.Send("Pager", new UMC.Web.WebMeta().Put("model", model, "cmd", cmd), true);
                            }

                        }
                        break;
                }
            }
        }
        void OpenUrl(Uri url)
        {

            var meta = new UMC.Web.WebMeta();
            meta["type"] = "OpenUrl";
            meta["value"] = url.AbsoluteUri;
            this.Context.Send(meta, true);
        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            var svalue = this.AsyncDialog("Url", d =>
            {
                this.Context.Send("Scanning", true);
                return this.DialogValue("none");
            });

            if (svalue.StartsWith("http://") || svalue.StartsWith("https://"))
            {
                var d = new Uri(svalue);
                this.Scanning(d);
                this.OpenUrl(d);
            }
            this.Prompt(svalue + "未处理");
        }
    }

}
