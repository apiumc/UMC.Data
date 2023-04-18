using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web;
using System.IO;

namespace UMC.Activities
{
    class SystemDirActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            if (request.IsCashier == false)
            {
                this.Prompt("只有管理员权限才可浏览服务器本地目录");
            }
            var key = this.AsyncDialog("Key", g => this.DialogValue("Root"));
            var file = this.AsyncDialog("File", g =>
            {
                var dir = this.AsyncDialog("Dir", g =>
                {
                    return this.DialogValue(UMC.Data.Reflection.ConfigPath("Static\\"));
                });
                var type = this.AsyncDialog("type", "File");
                var filter = this.AsyncDialog("filter", "*.*");

                var limit = this.AsyncDialog("limit", "none");
                request.Arguments.Remove("limit");
                if (limit == "none")
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, request.Arguments)
                        .CloseEvent("UI.Event")
                            .Builder(), true);
                }
                UISection ui;

                UISection ui2;

                if (String.Equals(dir, "none"))
                {
                    var uTitle = new UITitle("资源浏览器");
                    ui = UISection.Create(uTitle);
                    ui2 = ui;

                    var des = System.IO.DriveInfo.GetDrives();
                    foreach (var dr in des)
                    {
                        if (dr.RootDirectory.FullName.StartsWith("/System") == false)
                        {

                            var data = new WebMeta().Put("text", dr.RootDirectory.Name).Put("Icon", "\uf0a0");
                            var cell = UICell.Create("UI", data);
                            ui2.Add(cell);
                            data.Put("click", new UIClick(new WebMeta().Put("Dir", dr.RootDirectory.FullName)) { Key = "Query" });
                            cell.Style.Name("text").Color(0x111);

                        }
                    }
                }
                else
                {
                    var dirInfo = new System.IO.DirectoryInfo(dir);
                    ui = UISection.Create(new UITitle(dirInfo.Name));
                    if (dirInfo.Parent != null)
                    {
                        ui.AddCell('\uf112', "上级目录", "../", new UIClick("Dir", dirInfo.Parent.FullName) { Key = "Query" });
                    }
                    else
                    {
                        ui.AddCell('\uf112', "上级目录", "../", new UIClick("Dir", "none") { Key = "Query" });
                    }
                    ui2 = ui.NewSection();

                    try
                    {
                        var dirs = dirInfo.GetDirectories();
                        foreach (var dr in dirs)
                        {
                            if (dr.Name[0] != '.')
                            {
                                var data = new WebMeta().Put("text", dr.Name).Put("Icon", "\uf115");
                                var cell = UICell.Create("UI", data);
                                ui2.Add(cell);

                                data.Put("click", new UIClick(new WebMeta().Put("Dir", dr.FullName)) { Key = "Query" });
                                switch (type)
                                {
                                    case "File":
                                        break;
                                    default:
                                        cell.Style.Name("text").Click(new UIClick(new WebMeta().Put("Key", key).Put("File", dr.FullName)).Send(request.Model, request.Command)).Color(0x337ab7);
                                        break;
                                }

                            }


                        }
                    }
                    catch
                    {

                    }
                    switch (type)
                    {
                        case "File":

                            var uifile = ui.NewSection();
                            try
                            {
                                var filters = filter.Split(',');
                                var files = dirInfo.GetFiles();

                                foreach (var file in files)
                                {
                                    if (file.Name[0] != '.')
                                    {
                                        foreach (var f in filters)
                                        {
                                            if (f == "*.*" || f.EndsWith(file.Extension, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                uifile.AddCell('\uf0f6', file.Name, Utility.GetBitSize(file.Length), new UIClick(new WebMeta().Put("Key", key).Put("File", file.FullName)).Send(request.Model, request.Command));
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                            catch
                            {

                            }
                            break;
                    }
                }



                response.Redirect(ui);
                return this.DialogValue("none");


            }); ;
            var dic = new System.IO.DirectoryInfo(file);// UMC.Data.DataFactory.Instance().Organize(UMC.Data.Utility.IntParse(OrganizeId, 0));

            this.Context.Send(new UMC.Web.WebMeta().UIEvent(key, new ListItem()
            {
                Value = dic.FullName,
                Text = dic.Name
            }), true); ;

        }
    }
}
