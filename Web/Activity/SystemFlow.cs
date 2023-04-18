using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Activities;
using UMC.Data;
[assembly: UMC.Web.Mapping]


namespace UMC.Web.Activity
{

    [Mapping("System", Desc = "UMC基础组件")]
    public class SystemFlow : WebFlow
    {
        public override WebActivity GetFirstActivity()
        {
            switch (this.Context.Request.Command)
            {
                case "Dir":
                    return new SystemDirActivity();
                case "License":
                    return new SystemLicenseActivity();
                case "Log":
                    return new SystemLogActivity();
                case "Cache":
                    return new SystemCacheActivity();
                case "Root":
                    return new SystemRootActivity();
                case "Config":
                    return new SystemConfigActivity();
                case "Icon":
                    return new SystemIconActivity();
                case "Web":
                    return new SystemWebActivity();
                case "Picture":
                    return new SystemPictureActivity();
                case "Resource":
                    return new SystemResourceActivity();

            }
            return WebActivity.Empty;
        }

    }
}
