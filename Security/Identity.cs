using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Security
{


    /// <summary>
    /// 用户
    /// </summary>
    public abstract class Identity : System.Security.Principal.IIdentity, System.Security.Principal.IPrincipal
    {
        public static Identity Create(string name)
        {
            return new Identition(null, null, name);
        }
        public static Identity Create(string name, string alias)
        {
            return new Identition(null, alias, name);
        }

        public static Identity Create(Guid sn, string name, string alias, params string[] roels)
        {
            return new Identition(sn, alias, name, roels);
        }
        class Identition : Identity
        {
            public Identition(Guid? sn, string alias, string name, params string[] roels)
            {
                this.Id = sn;
                this._Alias = alias;
                this._Name = name;
                this.Roles = roels;
            }
            string _Alias;
            public override string Alias
            {
                get
                {
                    return _Alias;
                }
            }

            string _Name;
            public override string Name
            {
                get { return this._Name; }
            }


        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public virtual Guid? Id
        {
            get;
            protected set;
        }
        /// <summary>
        /// 别名全名
        /// </summary>
        public abstract string Alias
        {
            get;
        }
        /// <summary>
        /// 用户名
        /// </summary>
        public abstract string Name
        {
            get;
        }

        protected internal virtual string[] Roles
        {
            get;
            set;
        }

        public string AuthenticationType
        {
            get { return "UMC"; }
        }

        public bool IsAuthenticated
        {
            get
            {
                if (String.IsNullOrEmpty(this.Name) == false && String.Equals(this.Name, "?") == false)
                {
                    return true;
                }
                else
                { 
                    return false;
                }
            }
        }

        #region IPrincipal Members

        System.Security.Principal.IIdentity System.Security.Principal.IPrincipal.Identity
        {
            get { return this; }
        }

        bool System.Security.Principal.IPrincipal.IsInRole(string role)
        {
            var Roles = this.Roles;
            if (Roles == null)
            {
                return false;
            }
            else
            {
                role = role.ToLower();
                foreach (var r in Roles)
                {
                    if (r == Membership.AdminRole)
                    {
                        return true;
                    }
                    else
                    {
                        if (role == r.ToLower())
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        #endregion
    }

    public class Guest : Identity
    {
        public Guest(Guid? sn)
        {
            this.Id = sn;
            this.Roles = new string[0];
        }
        public override string Alias
        {
            get
            {
                return String.Empty;
            }
        }

        public override string Name
        {
            get { return "?"; }
        }
    }
}
