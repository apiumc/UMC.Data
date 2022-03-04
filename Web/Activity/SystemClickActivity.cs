using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace UMC.Web.Activity
{
    class SystemClickActivity : WebActivity
    {

        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var code = this.AsyncDialog("Code", g =>
            {
                var t = new UITextDialog() { Title = "点击码" };
                return t;
            });

            Hashtable hashtable;
            var url = UMC.Data.Utility.Scanning(code, request, out hashtable);
            if (url != null)
            {
                this.Context.Send(new UMC.Web.WebMeta().Put("value", url.AbsoluteUri).Put("type", "OpenUrl"), true);
            }
            if (hashtable != null)
            {
                var user = this.Context.Token.Identity();
                if (user.IsAuthenticated == false)
                {
                    response.Redirect("Account", "Login");
                }
                var model = hashtable["model"] as string;
                var cmd = hashtable["cmd"] as string;
                if (String.IsNullOrEmpty(model) == false && String.IsNullOrEmpty(cmd) == false)
                {
                    if (hashtable.ContainsValue("send"))
                    {
                        var send = hashtable["send"];
                        if (send is Hashtable)
                        {
                            var pos = new UMC.Web.WebMeta(send as Hashtable);

                            response.Redirect(model, cmd, pos, true);
                        }
                        else
                        {
                            response.Redirect(model, cmd, send.ToString());
                        }
                    }
                    else
                    {

                        response.Redirect(model, cmd);
                    }
                }
                else
                {

                    this.Prompt("不能识别，此指令");
                }

            }
            else
            {
                this.Prompt("未检测到此指令");
            }


        }


    }
}