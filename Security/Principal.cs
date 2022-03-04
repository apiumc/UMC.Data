using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Security
{
    /// <summary> 
    /// 线程身份
    /// </summary>
    public class Principal : System.Security.Principal.IPrincipal
    {
        protected Principal(Guid? appKey)
        {
            this._AppKey = appKey;
        }
        private Principal(Principal principal)
        {
            if (principal != null)
            {
                this._AppKey = principal._AppKey;
                this._Items = new System.Collections.Hashtable(principal._Items);
                this._identity = principal._identity;
                //this._SpecificData = principal._SpecificData;
                this.Status = principal.Status;
            }
        }
        System.Collections.Hashtable _Items = new System.Collections.Hashtable();
        /// <summary>
        /// 线程会话存储器
        /// </summary>
        public System.Collections.IDictionary Items
        {
            get
            {
                return _Items;
            }

        }
        public static Principal Current
        {
            get
            {
                //System.Threading.Tasks.Task.CurrentId
                var t = System.Threading.Thread.CurrentPrincipal as Principal;
                if (t == null)
                {
                    System.Threading.Thread.CurrentPrincipal = t = new Principal(Guid.Empty);
                }
                return t;
            }
        }

        /// <summary>
        /// 创建APPKey标识当前线程
        /// </summary>
        /// <param name="appKey">应用</param>
        /// <returns></returns>
        public static Principal New(Guid? appKey, Identity user)
        {
            var p = new Principal(appKey);
            p._identity = user;
            System.Threading.Thread.CurrentPrincipal = p;
            return p;
        }
        /// <summary>
        /// 创建APPKey标识当前线程
        /// </summary>
        /// <param name="appKey">应用</param>
        /// <returns></returns>
        public static Principal Create(Guid? appKey)
        {

            var princ = new Principal(System.Threading.Thread.CurrentPrincipal as Principal);
            princ._AppKey = appKey;
            System.Threading.Thread.CurrentPrincipal = princ;
            return princ;
        }
        /// <summary>
        /// 创建线程关联的状态
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static Principal Create(long status)
        {
            var princ = new Principal(System.Threading.Thread.CurrentPrincipal as Principal);
            princ.Status = status;
            System.Threading.Thread.CurrentPrincipal = princ;
            return princ;
        }
        /// <summary>
        /// 创建线程身体关联的用户
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        //public static Principal Create(System.Security.Principal.IIdentity identity)
        //{

        //    var princ = new Principal(System.Threading.Thread.CurrentPrincipal as Principal);
        //    princ._identity = identity;
        //    System.Threading.Thread.CurrentPrincipal = princ;
        //    return princ;
        //}
        /// <summary>
        /// 状态
        /// </summary>
        public Int64? Status
        {
            get;
            private set;
        }
        /// <summary>
        /// 创建线程身体关联的用户，并保留用户的凭证
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="token">用户凭证</param>
        /// <returns></returns>
        //public static Principal Create(System.Security.Principal.IIdentity identity, UMC.Security.AccessToken token)
        //{
        //    var princ = new Principal(System.Threading.Thread.CurrentPrincipal as Principal);
        //    princ._identity = identity;
        //    princ._SpecificData = token;
        //    System.Threading.Thread.CurrentPrincipal = princ;

        //    return princ;
        //}
        /// <summary>
        /// 
        /// </summary>
        System.Security.Principal.IIdentity _identity;
        Guid? _AppKey;
        /// <summary>
        /// 
        /// </summary>
        public Guid? AppKey
        {
            get
            {
                return this._AppKey;
            }
        }
        //UMC.Security.AccessToken _SpecificData;
        ///// <summary>
        /////  身份数据
        ///// </summary>
        //public UMC.Security.AccessToken Token
        //{
        //    get 
        //    {
        //        return _SpecificData;
        //    }
        //}
        #region IPrincipal 成员

        /// <summary>
        /// 
        /// </summary>
        public virtual System.Security.Principal.IIdentity Identity
        {
            get
            {
                return _identity;
            }
        }
        /// <summary>
        /// 验证角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns></returns>
        public virtual bool IsInRole(string role)
        {
            var id = this.Identity as System.Security.Principal.IPrincipal;
            if (id == null)
            {
                return false;
            }
            else
            {
                return id.IsInRole(role);
            }
        }

        #endregion
    }
}
