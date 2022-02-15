using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web.Activity
{

    [Mapping("System", Desc = "UMC基础组件")]
    class SystemFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "License":
                    return new SystemLicenseActivity();
                case "Cache":
                    return new SystemCacheActivity();
                case "Dir":
                    return new SystemDirActivity();
                case "Config":
                    return new SystemConfigActivity();
                case "Icon":
                    return new SystemIconActivity();
                case "TimeSpan":
                    this.Context.Response.Redirect(new WebMeta().Put("time", Utility.TimeSpan()));
                    break;
                case "Start":
                    this.Context.Send("Setup", true);
                    break;
                case "Web":
                    return new SystemWebActivity();
                case "Mapping":

                    var initializers = Data.Sql.Initializer.Initializers();
                    var database = Reflection.Configuration("database") ?? new Data.ProviderConfiguration();

                    var data = new System.Data.DataTable();
                    data.Columns.Add("name");
                    data.Columns.Add("text");
                    data.Columns.Add("setup", typeof(bool));
                    foreach (var n in initializers)
                    {
                        data.Rows.Add(n.Name, n.Caption, database.Providers.ContainsKey(n.ProviderName));
                    }
                    var data2 = new System.Data.DataTable();
                    data2.Columns.Add("model");
                    data2.Columns.Add("cmd");
                    data2.Columns.Add("text");
                    data2.Columns.Add("src");

                    var cfg = Data.Reflection.Configuration("UMC");
                    if (cfg != null)
                    {
                        for (var i = 0; i < cfg.Count; i++)
                        {
                            var p = cfg[i];
                            var cmd = p.Type;

                            if (String.IsNullOrEmpty(p.Type))
                            {
                                cmd = "*";
                            }

                            data2.Rows.Add(p.Name, cmd, p["desc"], p["src"]);
                        }
                    }
                    this.Context.Response.Redirect(new WebMeta().Put("component", data).Put("data", WebServlet.Mapping()).Put("webs", data2));
                    break;
                case "Menu":
                    return new SystemMenuActivity();
                case "Log":
                    return new SystemLogActivity();
                case "Setup":
                    return new SystemSetupActivity();
                case "Upgrade":
                    return new SystemUpgradeActivity();
                case "BackUp":
                    return new SystemBackUpActivity();
                case "Click":
                    return new SystemClickActivity();
                case "Restore":
                    return new SystemRestoreActivity();
            }
            return WebActivity.Empty;
        }

    }
}
