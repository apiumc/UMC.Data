using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data.Sql;
using UMC.Data.Entities;

namespace UMC.Data
{
    /// <summary>
    /// 会话对象
    /// </summary>
    /// <typeparam name="T">会员类型</typeparam>
    public class Session<T>
    {
        /// <summary>
        /// 会话值
        /// </summary>
        public T Value
        {
            get;
            private set;
        }
        /// <summary>
        /// 会话Key
        /// </summary>
        public string Key
        {
            get;
            private set;
        }
        public string ClientIP
        {
            get;
            private set;
        }
        public string ContentType
        {
            get;
            set;
        }
        private Guid _user_id;

        public Guid UserId
        {
            get
            {
                return _user_id;
            }
        }
        public Session(string sessionKey)
        {
            this.Key = sessionKey;
            var se = DataFactory.Instance().Session(this.Key);
            if (se != null)
            {
                this.ContentType = se.ContentType;

                if (typeof(T) == typeof(string))
                {
                    object obj = se.Content;
                    this.Value = (T)obj;
                }
                else
                {
                    this.Value = UMC.Data.JSON.Deserialize<T>(se.Content);
                }
                this._user_id = se.user_id ?? Guid.Empty;
                this.ModifiedTime = se.UpdateTime ?? DateTime.MinValue;
                this.ClientIP = se.ClientIP;

            }
        }

        public DateTime ModifiedTime
        {
            get;
            private set;
        }
        public Session(T value, string sessionKey)
        {
            this.Value = value;
            this.Key = sessionKey;
        }
        /// <summary>
        /// 提交更改
        /// </summary>
        public void Commit(string clientIp)
        {
            this.Commit(this._user_id, this.ContentType, false, clientIp);
        }

        /// <summary>
        /// 提交更改,且消除用户contentType类型的Session
        /// </summary>
        public void Commit(UMC.Security.Identity id, string contentType, string clientIp)
        {
            //this.ContentType = contentType;
            this.Commit(id.Id ?? Guid.Empty, contentType, true, clientIp);
        }
        public void Post(UMC.Security.Identity id, string contentType)
        {
            //this.ContentType = contentType;
            var session = new Session
            {
                UpdateTime = DateTime.Now,
                user_id = id.Id.Value,
                ContentType = contentType,// this.ContentType ?? "Settings",
                SessionKey = this.Key
            };
            if (this.Value is string)
            {
                session.Content = this.Value as string;
            }
            else
            {
                session.Content = UMC.Data.JSON.Serialize(this.Value, "ts");
            }
            this.ModifiedTime = DateTime.Now;

            DataFactory.Instance().Post(session);
        }
        public void Commit(T value, UMC.Security.Identity id, string clientIp)
        {
            this.Value = value;
            this.Commit(id, "Settings", clientIp);
        }

        public void Commit(T value, Guid uid, bool unique, string clientIp)
        {
            this.Value = value;
            this.Commit(uid, this.ContentType, unique, clientIp);
        }
        /// <summary>
        /// 提交更改
        /// </summary>
        public void Commit(Guid uid, String contentType, bool unique, string clientIp)
        {
            var session = new Session
            {
                UpdateTime = DateTime.Now,
                user_id = uid,
                ContentType = contentType,//this.ContentType,
                SessionKey = this.Key,
                ClientIP = clientIp
            };
            if (this.Value is string)
            {
                session.Content = this.Value as string;
            }
            else
            {
                session.Content = UMC.Data.JSON.Serialize(this.Value, "ts");
            }
            this.ModifiedTime = DateTime.Now;
            if (unique)
            {
                var v = DataFactory.Instance().Session(session.user_id.Value);

                foreach (var k in v)
                {
                    if (String.Equals(k.ContentType, this.ContentType))
                    {
                        DataFactory.Instance().Delete(k);
                    }
                }

            }
            DataFactory.Instance().Put(session);

        }
    }

}
