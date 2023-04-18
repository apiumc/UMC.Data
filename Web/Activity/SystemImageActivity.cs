
using System;
using System.Collections;
using UMC.Web;
namespace UMC.Web.Activity
{
    [Mapping("System", "Image", Auth = WebAuthType.All, Desc = "富文本编辑", Weight = 0)]
    public class SystemImageActivity : WebActivity
    {

        protected UIClick Click(UIClick ui)
        {
            String type = this.AsyncDialog("Click", g =>
            {
                var shett = new Web.UISheetDialog() { Title = "关联功能" };
                var request = this.Context.Request;
                shett.Put(new ListItem("连接扫一扫", "Scanning"));
                shett.Put(new ListItem("连接拨号", "Tel"));
                shett.Put(new ListItem("连接网址", "Url"));

                return shett;
            });
            switch (type)
            {
                case "Scanning":
                    return UIClick.Scanning();
                case "Tel":
                    return UIClick.Tel(this.AsyncDialog("Tel", g =>
                    {
                        var di = new UIFormDialog();
                        di.Title = "拨号号码";
                        di.AddText("拨号号码", g, String.Empty);
                        di.Submit("确认", "Click");

                        return (UIDialog)di;
                    }));
                case "Url":
                    return UIClick.Url(new Uri(this.AsyncDialog("Url", g =>
                    {
                        var di = new UIFormDialog();
                        di.Title = "网址地址";
                        di.AddText("网址地址", g, String.Empty);
                        di.Submit("确认", "Click");
                        return (UIDialog)di;

                    })));


                default:
                case "Setting":

                    var c = UMC.Data.JSON.Deserialize(UMC.Data.JSON.Serialize(ui)) as Hashtable;
                    WebMeta settings = this.AsyncDialog(g =>
                    {
                        UIFormDialog di = new UIFormDialog();
                        di.Title = ("功能指令");
                        di.AddText("模块代码", "Model", (String)c["model"]);
                        di.AddText("指令代码", "Command", (String)c["cmd"]);
                        di.AddPrompt("此块内容为专业内容，请由工程师设置");

                        if (c.ContainsKey("send"))
                        {
                            Object send = c["send"];
                            if (send is String)
                            {
                                di.AddText("参数", "Send", (String)send).PlaceHolder("如果没参数，则用none");
                            }
                            else
                            {

                                di.AddText("参数", "Send", UMC.Data.JSON.Serialize(send)).PlaceHolder("如果没参数，则用none");
                            }
                        }
                        else
                        {

                            di.AddText("参数", "Send").PlaceHolder("如果没参数，则用none").NotRequired();
                        }

                        di.Submit("确认", "Click");
                        return di;
                    }, "Send");
                    UIClick click = new UIClick();
                    String Model = settings.Get("Model");
                    String Command = settings.Get("Command");
                    String Send = settings.Get("Send");
                    click.Send(Model, Command);

                    if ("none".Equals(Send, StringComparison.CurrentCultureIgnoreCase) == false)
                    {
                        if (String.IsNullOrEmpty(Send) == false)
                        {
                            if (Send.StartsWith("{"))
                            {
                                click.Send(UMC.Data.JSON.Deserialize<WebMeta>(Send));
                            }
                            else
                            {
                                click.Send(Send);
                            }
                        }
                    }
                    return click;
            }

        }

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var UI = this.AsyncDialog("UI", "none");
            var section = this.AsyncDialog("section", "-1");
            var row = this.AsyncDialog("row", "-1");

            var Type = this.AsyncDialog("Type", g =>
            {
                var shett = new Web.UISheetDialog() { Title = "图片操作" };
                shett.Put("点击连接", "Click");
                shett.Put("更换图片", "Reset");
                shett.Put("移除图片", "Del");
                return shett;
            });
            switch (Type)
            {
                case "Reset":

                    var media_id = this.AsyncDialog("media_id", m =>
                    {
                        return Web.UIDialog.CreateDialog("File");
                    });

                    var url = new Uri(media_id);
                    var urlKey = String.Format("UserResources/{0:YYMMDD}/{1}{2}", DateTime.Now, UMC.Data.Utility.TimeSpan(), url.AbsolutePath.Substring(url.AbsolutePath.LastIndexOf('/')));
                    var webr = UMC.Data.WebResource.Instance();

                    webr.Transfer(url, urlKey);
                    var posmata = new UMC.Web.WebMeta();
                    posmata.Put("src", new Uri(request.Url, String.Format("/{0}", urlKey)).AbsoluteUri);
                    var vale = new UMC.Web.WebMeta().Put("section", section).Put("row", row).Put("method", "VALUE").Put("reloadSinle", true).Put("value", posmata);
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", UI, vale), true);
                    break;
                case "Del":
                    var dvale = new UMC.Web.WebMeta().Put("section", section).Put("row", row).Put("method", "DEL").Put("reloadSinle", true).Put("value", new UMC.Web.WebMeta());
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", UI, dvale), true);
                    break;
                case "Click":
                    var click = this.Click(new UIClick());
                    var posmata2 = new UMC.Web.WebMeta();
                    posmata2.Put("click", click);
                    this.Prompt("图片点击设置成功", false);
                    this.Context.Send("Click", false);
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", UI, new UMC.Web.WebMeta().Put("section", section).Put("row", row).Put("method", "VALUE")
                        .Put("value", posmata2)), true);
                    break;
            }

        }

    }

}