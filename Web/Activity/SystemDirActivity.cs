using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Data;
using UMC.Data.Entities;
using UMC.Web.UI;
using UMC.Web;

namespace UMC.Web.Activity
{

    class SystemDirActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var prefix = this.AsyncDialog("dir", g =>
            {
                return this.DialogValue("/");
            });
            var dir = UMC.Data.Reflection.ConfigPath("Static\\" + prefix);
            var media_id = this.AsyncDialog("media_id", "none");
            if (String.Equals("none", media_id) == false)
            {
                var sourceKey = String.Format("TEMP/Static/{0}", media_id);
                var http = new System.Net.Http.HttpClient();

                UMC.Data.Utility.Copy(http.GetStreamAsync(new Uri("http://oss.365lu.cn/" + sourceKey)).Result, dir);
                response.Redirect(new WebMeta().Put("src", "/" + prefix.Trim('\\', '/').Replace(System.IO.Path.DirectorySeparatorChar, '/')));
            }

            if (prefix == "/")
            {
                prefix = null;
            }
            var type = this.AsyncDialog("type", g =>
            {
                return this.DialogValue("list");
            });
            if (type == "Del")
            {
                if (request.IsMaster == false)
                {
                    this.Prompt("非管理员，只能查看");
                }
                var isDir = prefix.EndsWith(System.IO.Path.DirectorySeparatorChar + "");
                this.AsyncDialog("Config", g => new UMC.Web.UIConfirmDialog(isDir ? "您确认删除文件夹吗" : "您确认删除文件吗"));
                if (String.IsNullOrEmpty(prefix) == false)
                {
                    if (isDir)
                    {

                        System.IO.Directory.Delete(dir, true);
                    }
                    else
                    {

                        System.IO.File.Delete(dir);
                    }
                    this.Context.Send("reload", true);
                }
            }

            if (System.IO.Directory.Exists(dir) == false)
            {
                response.Redirect(new WebMeta().Put("prefix", (string)prefix));
            }
            var paths = System.IO.Directory.GetDirectories(dir);

            var dirs = new List<WebMeta>();
            var path = UMC.Data.Reflection.ConfigPath("Static\\");
            foreach (var file in paths)
            {
                var key = file.Substring(path.Length);
                var name = key;
                if (string.IsNullOrEmpty(prefix) == false)
                {
                    name = key.Substring(prefix.Length);
                }

                dirs.Add(new WebMeta().Put("name", name.Trim(System.IO.Path.DirectorySeparatorChar)).Put("dir", key + System.IO.Path.DirectorySeparatorChar));

            }



            var Summaries = System.IO.Directory.GetFiles(dir);
            var files = new List<WebMeta>();
            foreach (var file in Summaries)
            {
                var key = file.Substring(path.Length);
                var summ = new System.IO.FileInfo(file);
                var name = key;

                if (string.IsNullOrEmpty(prefix) == false)
                {
                    name = key.Substring(prefix.Length);
                }

                files.Add(new WebMeta().Put("href", new Uri(request.Url, "/" + key.Replace(System.IO.Path.DirectorySeparatorChar, '/')).AbsoluteUri).Put("size", summ.Length).Put("name", name.Trim(System.IO.Path.DirectorySeparatorChar), "file", key).Put("time", summ.LastWriteTime.ToLocalTime().ToString()));
            }

            var data = new WebMeta().Put("dir", dirs).Put("files", files).Put("prefix", (string)prefix);
            if (string.IsNullOrEmpty((string)prefix) == false)
            {
                var pre = prefix.Trim(System.IO.Path.DirectorySeparatorChar);
                var index = pre.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                if (index > -0)
                {
                    data.Put("pre", pre.Substring(0, index + 1));
                    data.Put("name", prefix.Substring(index + 1).Trim(System.IO.Path.DirectorySeparatorChar));
                }
                else
                {
                    data.Put("name", prefix.Trim(System.IO.Path.DirectorySeparatorChar));

                }
            }
            response.Redirect(data);

        }

    }

}