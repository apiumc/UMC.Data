using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Data.Sql;

namespace UMC.Web.Activity
{

    class SystemUpgradeActivity : WebActivity
    {
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {

            if (request.IsMaster == false)
            {
                this.Prompt("只有管理员才能检测升级");
            }
            var Initializers = Data.Sql.Initializer.Initializers();
            var model = this.AsyncDialog("Model", g =>
            {
                if (Initializers.Length == 0)
                {
                    return this.DialogValue(Initializers[0].Name);
                }
                else
                {
                    var fm = new UISheetDialog() { Title = "选择检测升级组件" };
                    foreach (var v in Initializers)
                    {
                        fm.Options.Add(new UIClick(v.Name) { Text = v.Caption }.Send(request.Model, request.Command));

                    }
                    return fm;
                }

            });
            var Key = Utility.Guid(Guid.NewGuid());
            var log = new UMC.Data.CSV.Log(Utility.GetRoot(request.Url), Key, "开始检测");

            var Hask = new Dictionary<String, DbProvider>();
            foreach (var initer in Initializers)
            {
                Hask[initer.ProviderName] = UMC.Data.Database.Instance(initer.ProviderName).DbProvider;
            }
            Data.Reflection.Start(() =>
            {
                try
                {
                    var now = DateTime.Now;

                    var database = Reflection.Configuration("database") ?? new UMC.Data.ProviderConfiguration();
                    foreach (var initer in Initializers)
                    {
                        if (String.Equals(initer.Name, model))
                        {
                            if (database.Providers.ContainsKey(initer.ProviderName))
                            {

                                log.Info("正在升级", initer.Caption);
                                initer.Check(log, Hask[initer.ProviderName]);
                            }

                            else
                            {
                                log.Info("未安装", initer.Caption);
                            }
                        }

                    }
                    log.End("检测升级已完成");

                }
                catch (Exception ex)
                {
                    log.End("安装失败");
                    log.Info(ex.Message);

                }

            });
            this.Context.Send("Initializer", false);

            response.Redirect("System", "Log", Key);
        }

    }
}
