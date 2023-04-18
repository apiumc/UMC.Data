using System;
using System.Collections;
using System.IO;
using UMC.Data;
using UMC.Net;
using UMC.Web;
namespace UMC.Web.Activity
{

    [Mapping("System", "Resource", Auth = WebAuthType.All, Desc = "上传资源", Weight = 0)]
    public class SystemResourceActivity : UMC.Web.WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var webr = WebResource.Instance();
            var Key = this.AsyncDialog("Key", "WebResource");

            var media_id = this.AsyncDialog("media_id", g =>
            {
                if (request.IsApp)
                {
                    return Web.UIDialog.CreateDialog("File");
                }
                else if (Key.IndexOf("/") > 0)
                {
                    return this.DialogValue("none");
                }
                else
                {
                    var from = new Web.UIFormDialog() { Title = "资源上传" };

                    from.AddFile("选择资源", "media_id", String.Empty);

                    from.Submit("确认上传", "UI.Event");
                    return from;
                }
            });
            var root = request.SendValues?["root"];
            if (String.IsNullOrEmpty(root))
            {
                root = Utility.Parse36Encode(this.Context.Token.UserId.Value); ;
            }
            if (media_id.StartsWith("http://") || media_id.StartsWith("https://"))
            {
                var url = new Uri(media_id);
                var name = Uri.UnescapeDataString(url.AbsolutePath.Substring(url.AbsolutePath.LastIndexOf('/') + 1));

                var urlKey = String.Format("UserResources/{0}/{1}/{2}", root, Utility.TimeSpan(), name);

                webr.Transfer(url, urlKey);

                var domain = webr.WebDomain();
                var posmata = new WebMeta().Put("name", name);
                if (String.IsNullOrEmpty(domain) || domain.Length == 1)
                {
                    posmata.Put("src", new Uri(request.Url, String.Format("/{0}", urlKey)).AbsoluteUri);
                }
                else
                {
                    posmata.Put("src", $"{request.Url.Scheme}://{domain}/{urlKey}");
                }

                posmata.Put("Text", name).Put("Value", $"/{urlKey}");
                this.Context.Send(new WebMeta().UIEvent(Key, this.AsyncDialog("UI", "none"), posmata), true);
            }
            else if (media_id.StartsWith("/TEMP/"))
            {
                var name = Uri.UnescapeDataString(media_id.Substring(media_id.LastIndexOf('/') + 1));
                var urlKey = String.Format("UserResources/{0}/{1}/{2}", root, Utility.TimeSpan(), name);

                string filename = UMC.Data.Reflection.ConfigPath(String.Format("Static{0}", media_id));
                if (System.IO.File.Exists(filename))
                {
                    using (System.IO.Stream sWriter = File.OpenRead(filename))
                    {
                        webr.Transfer(sWriter, urlKey);
                    }
                    var posmata = new WebMeta().Put("name", name);
                    var domain = webr.WebDomain();
                    if (String.IsNullOrEmpty(domain) || domain.Length == 1)
                    {
                        posmata.Put("src", new Uri(request.Url, String.Format("/{0}", urlKey)).AbsoluteUri);
                    }
                    else
                    {
                        posmata.Put("src", $"{request.Url.Scheme}://{domain}/{urlKey}");
                    }
                    posmata.Put("Text", name).Put("Value", $"/{urlKey}");
                    this.Context.Send(new WebMeta().UIEvent(Key, this.AsyncDialog("UI", "none"), posmata), true);


                }
            }
            this.Prompt("网络资源不能识别");


        }

    }
}