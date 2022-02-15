using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Net
{
    public abstract class Message : UMC.Data.DataProvider
    {
        static Message _instance;
        private class Messager : Message
        {
            public override void Send(String type, Hashtable content, string number)
            {

            }

            public override void Send(string content, params string[] to)
            {

            }
        }
        public static void Instance(Message message, Provider provider)
        {
            message.Provider = provider;
            _instance = message;
        }

        public static Message Instance()
        {
            if (_instance == null)
            {
                _instance = UMC.Data.Reflection.CreateObject("Message") as Message;
                if (_instance == null)
                {
                    _instance = new Messager();
                    _instance.Provider = Data.Provider.Create("Message", "UMC.Data.Message");
                }
            }
            return _instance;
        }

        #region IMessage Members

        /// <summary>
        /// 短信模板发送
        /// </summary>
        /// <param name="type">对应模板</param>
        /// <param name="data">替换的字典</param>
        /// <param name="mobile">手机号码</param>
        public abstract void Send(String type, System.Collections.Hashtable data, string mobile);
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="content">短信内容</param>
        /// <param name="to">手机号码</param>
        public abstract void Send(string content, params string[] to);
        public virtual void Send(System.Net.Mail.MailMessage message)
        {
            var client = new System.Net.Mail.SmtpClient(this.Provider.Attributes["Host"]
                , UMC.Data.Utility.IntParse(this.Provider.Attributes["Port"], 25));
            if (!String.IsNullOrEmpty(this.Provider.Attributes["Username"]) && !String.IsNullOrEmpty(this.Provider.Attributes["Password"]))
            {
                client.Credentials = new System.Net.NetworkCredential(this.Provider.Attributes["Username"], this.Provider.Attributes["Password"]);
            }
            client.EnableSsl = this.Provider.Attributes["EnableSsl"] == "true";
            message.From = new System.Net.Mail.MailAddress(this.Provider["From"]);
            client.Send(message);
        }

        #endregion
    }
}
