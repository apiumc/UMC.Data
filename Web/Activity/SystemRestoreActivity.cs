using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Data.Sql;

namespace UMC.Web.Activity
{

    class SystemRestoreActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能还原");
            }

            var files = System.IO.Directory.GetFiles(UMC.Data.Utility.MapPath(String.Format("App_Data/BackUp/{0}", Utility.GetRoot(request.Url))), "*.umc", System.IO.SearchOption.TopDirectoryOnly);
            var index = Utility.IntParse(this.AsyncDialog("Index", g =>
            {
                var rd = new UIRadioDialog();
                rd.Title = "请选择备份";
                for (var i = 0; i < files.Length; i++)
                {

                    rd.Options.Add(System.IO.Path.GetFileName(files[i]), i.ToString());
                }
                if (files.Length == 0)
                {
                    this.Prompt("未有备份文件");
                }
                return rd;
            }), 0);
            var path = String.Format("BackUp/{0}/{1}", Utility.GetRoot(request.Url), System.IO.Path.GetFileName(files[index]));
            var Key = Utility.Guid(Guid.NewGuid());
            var log = new UMC.Data.CSV.Log(Utility.GetRoot(request.Url), Key, "开始备份");
            var appKey = Security.Principal.Current.AppKey ?? Guid.Empty;
            Data.Reflection.Start(() =>
            {
                var Initializers = Data.Sql.Initializer.Initializers();
                try
                {
                    var now = DateTime.Now;

                    var database = Reflection.Configuration("database") ?? new UMC.Data.ProviderConfiguration();
                    //var count = false;
                    foreach (var initer in Initializers)
                    {
                        if (database.Providers.ContainsKey(initer.ProviderName))
                        {

                            log.Info("正在还原", initer.Caption);
                            var provider2 = Reflection.CreateObject(Reflection.Instance().DatabaseProvider(initer.ProviderName, appKey)) as DbProvider;

                            initer.Restore(log, provider2, path);
                        }

                        else
                        {
                            log.Info("未安装", initer.Caption);
                        }


                    }


                    log.End("还原已完成");

                }
                catch (Exception ex)
                {
                    log.End("还原失败");
                    log.Info(ex.ToString());

                }

            });

            this.Context.Send("Initializer", false);

            response.Redirect("System", "Log", Key);
        }

    }
}
