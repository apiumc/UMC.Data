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
        public string ContentType
        {
            get;
            set;
        }
        private Guid _user_id;
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
                _user_id = se.user_id ?? Guid.Empty;

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
        public void Commit()
        {
            this.Commit(this._user_id, false);
        }
        /// <summary>
        /// 提交更改
        /// </summary>
        public void Commit(UMC.Security.Identity id)
        {
            this.Commit(id.Id ?? Guid.Empty, false);
        }
        /// <summary>
        /// 提交更改,且消除用户contentType类型的Session
        /// </summary>
        public void Commit(UMC.Security.Identity id, string contentType)
        {
            this.ContentType = contentType;
            this.Commit(id.Id ?? Guid.Empty, true);
        }
        public void Post(UMC.Security.Identity id, string contentType)
        {
            this.ContentType = contentType;
            var session = new Session
            {
                UpdateTime = DateTime.Now,
                user_id = id.Id.Value,
                ContentType = this.ContentType ?? "Settings",
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
        public void Commit(T value, UMC.Security.Identity id)
        {
            this.Value = value;
            this.Commit(id, "Settings");
        }

        public void Commit(T value, Guid uid, bool unique)
        {
            this.Value = value;
            this.Commit(uid, unique);
        }
        /// <summary>
        /// 提交更改
        /// </summary>
        public void Commit(Guid uid, bool unique)
        {
            var session = new Session
            {
                UpdateTime = DateTime.Now,
                user_id = uid,
                ContentType = this.ContentType ?? "Settings",
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
