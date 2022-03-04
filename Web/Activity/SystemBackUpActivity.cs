using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Data.Sql;

namespace UMC.Web.Activity
{

    class SystemBackUpActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能备份");
            }
            var path = String.Format("BackUp/{0}/{1:yy-MM-dd}.umc", Utility.GetRoot(request.Url), DateTime.Now);

            var file = UMC.Data.Utility.MapPath(String.Format("App_Data/{0}", path));// Utility.GetRoot(request.Url), DateTime.Now));

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(file)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
            }
            if (System.IO.File.Exists(file))
            {
                this.Prompt("一天只能备份一次");
            }
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

                            log.Info("正在备份", initer.Caption);
                            var provider2 = Reflection.CreateObject(Reflection.Instance().DatabaseProvider(initer.ProviderName, appKey)) as DbProvider;

                            initer.BackUp(log, provider2, path);
                        }

                        else
                        {
                            log.Info("未安装", initer.Caption);
                        }


                    }


                    log.End("备份已完成");

                }
                catch (Exception ex)
                {
                    log.End("备份失败");
                    log.Info(ex.Source);

                }

            });

            response.Redirect("System", "Log", Key);
        }

    }
}
