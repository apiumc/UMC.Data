
using System;
using System.Collections.Generic;
using UMC.Data;
using UMC.Web;
namespace UMC.Web.Activity
{
    [Mapping("System", "Picture", Auth = WebAuthType.All, Desc = "上传组图", Weight = 0)]
    public class SystemPictureActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var user = this.Context.Token.Identity();
            var groupId = UMC.Data.Utility.Guid(this.AsyncDialog("id", d =>
            {
                return this.DialogValue(user.Id.ToString());
            }), true) ?? Guid.Empty;


            var seq = UMC.Data.Utility.Parse(this.AsyncDialog("seq", "1"), 0);
            WebResource webr = WebResource.Instance();
            var media_id = this.AsyncDialog("media_id", g =>
            {
                if (request.IsApp)
                {
                    return Web.UIDialog.CreateDialog("File");
                }
                else
                {
                    var from = new Web.UIFormDialog() { Title = "图片上传" };
                    if (seq == 0)
                    {

                        from.AddFile("选择图片", "media_id", String.Empty);
                    }
                    else
                    {
                        from.AddFile("选择图片", "media_id", webr.ImageResolve(groupId, seq, 4));
                    }

                    from.Submit("确认上传", $"{request.Model}.{request.Command}");
                    return from;
                }
            });


            if (String.Equals(media_id, "none"))
            {
                var index = new List<byte>();
                var picture = Data.DataFactory.Instance().Picture(groupId) ?? new Data.Entities.Picture { group_id = groupId };
                if (picture.Value != null)
                {
                    index.AddRange(picture.Value);
                }

                this.AsyncDialog("Confirm", s =>
                {

                    return new Web.UIConfirmDialog(String.Format("确认删除此组第{0}张图片吗", seq)) { Title = "删除提示" };

                });

                if (seq == 1)
                {
                    if (index.Count > 1)
                    {
                        webr.Transfer(new Uri(webr.ImageResolve(picture.group_id.Value, index[1], 0)), groupId, seq);

                        index.RemoveAt(1);
                    }
                }
                else
                {
                    index.Remove(Convert.ToByte(seq));
                }
                if (index.Count == 0)
                {
                    Data.DataFactory.Instance().Delete(picture);
                }
                else
                {
                    picture.Value = index.ToArray();
                    picture.UploadTime = UMC.Data.Utility.TimeSpan();
                    Data.DataFactory.Instance().Put(picture);
                }

            }
            else
            {

                if (seq == 0)
                {
                    var index = new List<byte>();
                    var picture = Data.DataFactory.Instance().Picture(groupId) ?? new Data.Entities.Picture { group_id = groupId };
                    if (picture.Value != null)
                    {
                        index.AddRange(picture.Value);
                    }
                    if (index.Count > 0)
                    {
                        seq = index[index.Count - 1] + 1;

                        index.Add(Convert.ToByte(seq));
                    }
                    else
                    {
                        seq = 1;
                        index.Add(1);
                    }
                    picture.Value = index.ToArray();
                    picture.UploadTime = UMC.Data.Utility.TimeSpan();
                    Data.DataFactory.Instance().Put(picture);

                }
                var type = this.AsyncDialog("type", "none");

                if (media_id.StartsWith("http://") || media_id.StartsWith("https://"))
                {
                    var url = new Uri(media_id);

                    webr.Transfer(url, groupId, seq);
                    if (type == "none")
                    {
                        webr.Transfer(url, groupId, seq);
                    }
                    else
                    {
                        webr.Transfer(url, groupId, seq, type.ToLower());
                    }

                }
                else
                {

                    var file = UMC.Data.Reflection.ConfigPath(String.Format("Static\\TEMP\\{0}", media_id.Substring(media_id.IndexOf('/', 2) + 1)));

                    if (System.IO.File.Exists(file))
                    {
                        if (type == "none")

                        {
                            webr.Transfer(new Uri($"file://{file}"), groupId, seq);
                        }
                        else
                        {
                            webr.Transfer(new Uri($"file://{file}"), groupId, seq, type.ToLower());
                        }

                    }
                }
            }

            this.Context.Send(new WebMeta().Put("type", $"{request.Model}.{request.Command}").Put("id", groupId.ToString()), true);


        }


    }

}