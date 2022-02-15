using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Data.Sql;

namespace UMC.Web.Activity
{

    class SystemMenuActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能重置菜单");
            }
            this.AsyncDialog("Confirm", g => new UIConfirmDialog("将会清空现有菜单，同时清除菜单权限"));
            var Key = Utility.Guid(Guid.NewGuid());
            var log = new UMC.Data.CSV.Log(Utility.GetRoot(request.Url), Key, "开始重置菜单");
            //var p = Database.Instance().DbProvider;
            DataFactory.Instance().Delete(new Data.Entities.Menu { Site = 0 });
            //var factory = new DbFactory(Database.Instance().DbProvider);
            //factory.ObjectEntity<Data.Entities.Menu>().Delete();
            //factory.ObjectEntity<>
            //UMC.Data.Reflection.Start(() =>
            //   {
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

                        log.Info("正在检测", initer.Caption);
                        initer.Menu(new Hashtable());
                    }

                    else
                    {
                        log.Info("未安装", initer.Caption);
                    }


                }


                log.End("检测菜单已完成");

            }
            catch (Exception ex)
            {
                log.End("检测失败");
                log.Info(ex.Message);

            }

            //});//.Start();


            //response.Redirect("System", "Log", Key);
        }

    }
}
