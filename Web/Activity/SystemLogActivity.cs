using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Data;
using UMC.Data.Sql;
using UMC.Web;
using System.IO;

namespace UMC.Web.Activity
{

    class SystemLogActivity : UMC.Web.WebActivity
    {
        public override void ProcessActivity(UMC.Web.WebRequest request, UMC.Web.WebResponse response)
        {
            var media_id = this.AsyncDialog("Key", g =>
             {
                 this.Prompt("请输入Key");
                 return new UMC.Web.UITextDialog();
             });

            var file = Reflection.ConfigPath(String.Format("Static\\TEMP\\{0}.csv", media_id));
            if (System.IO.File.Exists(file) == false)
            {
                this.Prompt("不存在此日志");
            }
            else
            {
                var form = request.SendValues ?? new UMC.Web.WebMeta();
                if (form.ContainsKey("limit") == false)
                {
                    this.Context.Send(new UISectionBuilder(request.Model, request.Command, new WebMeta().Put("Key", media_id))
                        .Builder(), true);

                }
            }


            var ui = UISection.Create(new UITitle("执行日志"));
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);

                bool IsEnd = false;
                CSV.EachRow(reader, rows =>
                {
                    var cell = UMC.Web.UI.UICMS.Create("CMSText", new WebMeta().Put("text", String.Join(" ", rows)));
                    cell.Style.Size(12);
                    ui.Add(cell);
                    if (rows.Length > 0)
                    {
                        if (String.Equals(rows[0], "END", StringComparison.CurrentCultureIgnoreCase))
                        {
                            IsEnd = true;
                        }
                    }
                });
                if (IsEnd == false)
                {
                    ui.Add(UMC.Web.UI.UICMS.Create("UIRefreshing", new WebMeta().Put("time", 10)));
                }
            }
            response.Redirect(ui);
        }



    }
}