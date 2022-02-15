using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using UMC.Data;
using UMC.Data.Sql;
using UMC.Web;

namespace UMC.Web.Activity
{
    class SystemCacheActivity : UMC.Web.WebActivity
    {
        public override void ProcessActivity(UMC.Web.WebRequest request, UMC.Web.WebResponse response)
        {

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

                var data = UMC.Data.HotCache.Caches();

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    ui.AddCell(data.Rows[i][1].ToString(), String.Format("{0}条数", data.Rows[i][2].ToString()), new UIClick(g, data.Rows[i][0].ToString()).Send(request.Model, request.Command));
                }

                if (ui.Length == 0)
                {
                    ui.Add("Desc", new UMC.Web.WebMeta().Put("desc", "未有储存的数据").Put("icon", "\uEA05")
                , new UMC.Web.WebMeta().Put("desc", "{icon}\n{desc}"), new UIStyle().Align(1).Color(0xaaa).Padding(20, 20).BgColor(0xfff).Size(12).Name("icon", new UIStyle().Font("wdk").Size(60)));//.Name 

                }
                response.Redirect(ui);
                return this.DialogValue("none");
            });
            UMC.Data.HotCache.Clear(Key);

            this.Context.Send("Cache.Event", true);
        }


    }
}