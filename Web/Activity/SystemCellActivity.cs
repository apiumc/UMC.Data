using System;
using System.Collections;
using UMC.Data;
using UMC.Web;
namespace UMC.Web.Activity
{
    [Mapping("System", "Cell", Auth = WebAuthType.All, Desc = "富文本编辑", Weight = 0)]
    public class SystemCellActivity : WebActivity
    {
        WebMeta Text()
        {
            var webr = UMC.Data.WebResource.Instance();
            var data = new UMC.Web.WebMeta().Put("text", "插入文字");
            var cell = UICell.Create("CMSText", data);
            return new UMC.Web.WebMeta().Cell(cell);

        }
        WebMeta Image(WebRequest request)
        {
            var media_id = this.AsyncDialog("media_id", m =>
            {
                var f = Web.UIDialog.CreateDialog("File");
                f.Config.Put("Submit", new UIClick(new UMC.Web.WebMeta(request.Arguments.GetDictionary()).Put("media_id", "Value"))
                {
                    Command = request.Command,
                    Model = request.Model
                });
                return f;
            });
            var url = new Uri(media_id);
            var urlKey = String.Format("UserResources/{0:YYMMDD}/{1}{2}", DateTime.Now, UMC.Data.Utility.TimeSpan(), url.AbsolutePath.Substring(url.AbsolutePath.LastIndexOf('/')));
            var webr = UMC.Data.WebResource.Instance();


            var posmata = new UMC.Web.WebMeta();
            var cell = UMC.Web.UICell.Create("CMSImage", posmata);
            posmata.Put("src", $"{request.Url.Scheme}://{webr.WebDomain()}/{urlKey}");

            webr.Transfer(url, urlKey);
            cell.Style.Padding(10);
            return new UMC.Web.WebMeta().Cell(cell);

        }
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Key = this.AsyncDialog("Key", "WebResource");
            var UI = this.AsyncDialog("UI", "none");
            var Type = this.AsyncDialog("Type", gKey =>
            {
                return new Web.UISheetDialog() { Title = "插入" }.Put("插入图片", "Image").Put("插入文字", "Text");
                //return seett;

            });
            switch (Type)
            {
                case "Image":

                    this.Context.Send(new UMC.Web.WebMeta().UIEvent(Key, UI, Image(request)), true);
                    break;
                case "Text":
                    this.Context.Send(new UMC.Web.WebMeta().UIEvent(Key, UI, Text()), true);
                    break;
            }
        }
    }
}