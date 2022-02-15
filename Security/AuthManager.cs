using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Collections;
using UMC.Data.Entities;
using UMC.Data;

namespace UMC.Security
{
    /// <summary>
    /// 授权管理
    /// </summary>
    public class AuthManager
    {
        System.Security.Principal.IPrincipal principal;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="principal"></param>
        private AuthManager(System.Security.Principal.IPrincipal principal)
        {
            this.principal = principal;
        }

        /// <summary>
        /// 验证用户是否能通过通配符
        /// </summary>
        /// <param name="wildcard">通配符</param>
        /// <returns></returns>
        public static Boolean IsAuthorization(string wildcard)
        {
            return IsAuthorization(System.Threading.Thread.CurrentPrincipal, wildcard);
        }
        /// <summary>
        /// 批量验证权限
        /// </summary>
        public static bool[] IsAuthorization(params string[] wildcards)
        {
            return IsAuthorization(System.Threading.Thread.CurrentPrincipal, wildcards);
        }
        /// <summary>
        /// 批量验证权限
        /// </summary>
        /// <param name="wildcards"></param>
        /// <returns></returns>
        public static bool[] IsAuthorization(System.Security.Principal.IPrincipal princ, params string[] wildcards)
        {
            bool[] rerValue = new bool[wildcards.Length];
            if (wildcards.Length > 0)
            {
                if (princ.IsInRole(UMC.Security.Membership.AdminRole))
                {
                    for (var i = 0; i < rerValue.Length; i++)
                    {
                        rerValue[i] = true;
                    }
                    return rerValue;
                }
                var list = new List<String>();
                foreach (var wildcard in wildcards)
                {
                    list.Add(wildcard);
                    int l = wildcard.Length - 1;

                    while (l > -1)
                    {
                        switch (wildcard[l])
                        {
                            case '/':
                                list.Add(wildcard.Substring(0, l) + "/*");
                                break;
                        }
                        l--;
                    }
                }
                var wMger = new AuthManager(princ);
                var vs = wMger.Check(list.ToArray());
                int start = 0, end = 0;

                for (int i = 1; i < wildcards.Length; i++)
                {
                    end = list.FindIndex(w => wildcards[i] == w);
                    rerValue[i - 1] = IsAuthorization(vs, start, end);
                    start = end;
                }
                rerValue[wildcards.Length - 1] = IsAuthorization(vs, start, vs.Length);
            }
            return rerValue;
        }
        static bool IsAuthorization(int[] vs, int start, int end)
        {
            for (var c = start; c < end; c++)
            {
                if (vs[c] != 0)
                {
                    return vs[c] > 0;
                }
            }

            return true;
        }

        /// <summary>
        /// 验证权限符SId
        /// </summary>
        /// <param name="sid">权限符SId</param>
        public static bool IsAuthorization(Guid sid)
        {
            return IsAuthorization(sid.ToString());
        }
        /// <summary>
        /// 验证权限符SId,如果返回大于0或通过，如果等于0表示没有设置过，如果小于0表示没有通过
        /// </summary>
        /// <param name="sid">权限符SId</param>
        /// <param name="principal">用户身份</param>
        /// <returns></returns>
        public static int IsAuthorization(Guid sid, System.Security.Principal.IPrincipal principal)
        {
            if (principal == null)
            {
                throw new System.ArgumentNullException("principal");
            }
            if (!principal.IsInRole(UMC.Security.Membership.AdminRole))
            {
                AuthManager wMger = new AuthManager(principal);//as WildcardManager;
                return wMger.Check(sid.ToString())[0];
            }
            else
            {
                return 1;
            }
        }


        /// <summary>
        /// 复制权限
        /// </summary>
        /// <param name="sourceKey">源权限SID</param>
        /// <param name="destKey">目标权限SID</param>
        public static bool CopyAuthorize(string sourceKey, string destKey)
        {
            AuthManager wMger = new AuthManager(System.Threading.Thread.CurrentPrincipal);
            return wMger.Copy(sourceKey, destKey);
        }
        /// <summary>
        /// 解除权限设置
        /// </summary>
        /// <param name="wildcardKey"></param>
        public static void Unauthorize(string wildcardKey)
        {
            AuthManager wMger = new AuthManager(System.Threading.Thread.CurrentPrincipal);
            wMger.Delete(wildcardKey);
        }
        /// <summary>
        /// 验证用户是否能通过通配符
        /// </summary>
        /// <param name="wildcard">通配符</param>
        /// <param name="principal">用户身份</param>
        /// <returns></returns>
        public static bool IsAuthorization(System.Security.Principal.IPrincipal principal, string wildcard)
        {

            return IsAuthorization(System.Threading.Thread.CurrentPrincipal, new string[] { wildcard })[0];

        }



        int Check(UMC.Security.Authorize[] Authorizes)
        {
            int isAllowRoles = 0;
            int isAllowUser = 0;
            int isAllowOrganize = 0;
            var username = this.principal.Identity.Name;

            foreach (var dr in Authorizes)
            {

                var sValue = dr.Value;
                switch (dr.Type)
                {
                    case AuthorizeType.UserAllow:
                        switch (sValue)
                        {
                            case "*":
                                if (isAllowUser == 0)
                                {
                                    isAllowUser = 1;
                                };
                                break;
                            case "?":
                                if (this.principal.Identity.IsAuthenticated == false)
                                {
                                    isAllowUser = 1;
                                }
                                break;
                            default:
                                if (String.Equals(username, sValue, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    isAllowUser = 1;
                                }
                                break;
                        }
                        break;
                    case AuthorizeType.RoleAllow:
                        if (sValue == "*")
                        {
                            if (isAllowRoles == 0)
                            {
                                isAllowRoles = 1;
                            }
                        }
                        else if (isAllowRoles != 1)
                        {
                            if (principal.IsInRole(sValue))
                            {
                                isAllowRoles = 1;
                            }
                            else
                            {
                                isAllowRoles = -1;
                            }
                        }
                        break;
                    case AuthorizeType.UserDeny:
                        if (isAllowUser > -1)
                        {
                            switch (sValue)
                            {
                                case "*":
                                    if (isAllowUser == 0)
                                    {
                                        isAllowUser = -1;
                                    };
                                    break;
                                case "?":
                                    if (this.principal.Identity.IsAuthenticated == false)
                                    {
                                        isAllowUser = -1;
                                    }
                                    break;
                                default:
                                    if (String.Equals(username, sValue, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        isAllowUser = -1;
                                    }
                                    break;
                            }
                        }
                        break;
                    case AuthorizeType.OrganizeAllow:

                        if (sValue == "*")
                        {
                            if (isAllowOrganize == 0)
                            {
                                isAllowOrganize = 1;
                            }
                        }
                        else if (isAllowOrganize != 1)
                        {
                            if (this.principal.Identity is IOrganizeMember)
                            {
                                var omember = this.principal.Identity as IOrganizeMember;

                                if (omember.IsOrganizeMember(sValue))
                                {
                                    isAllowOrganize = 1;
                                }
                                else
                                {
                                    isAllowOrganize = -1;
                                }
                            }
                        }
                        break;
                    case AuthorizeType.OrganizeDeny:
                        if (sValue == "*")
                        {
                            if (isAllowOrganize == 0)
                            {
                                isAllowOrganize = -1;
                            }
                        }
                        else if (isAllowOrganize != -1)
                        {
                            if (this.principal.Identity is IOrganizeMember)
                            {
                                var omember = this.principal.Identity as IOrganizeMember;

                                if (omember.IsOrganizeMember(sValue))
                                {
                                    isAllowOrganize = -1;
                                }
                            }

                        }
                        break;
                    case AuthorizeType.RoleDeny:
                        if (sValue == "*")
                        {
                            if (isAllowRoles == 0)
                            {
                                isAllowRoles = -1;
                            }
                        }
                        else if (isAllowRoles != -1 && principal.IsInRole(sValue))
                        {
                            isAllowRoles = -1;

                        }
                        break;
                }
            }

            if (isAllowUser != 0)
            {
                return isAllowUser;

            }

            if (isAllowRoles != 0)
            {
                return isAllowRoles;

            }
            return isAllowOrganize;
            //if (isAllowOrganize == -1)
            //{
            //    if (isAllowRoles <= 0 && isAllowUser <= 0)
            //    {
            //        return -1;
            //    }

            //}
            //if (isAllowRoles == -1)
            //{
            //    if (isAllowUser <= 0)
            //    {
            //        return -1;
            //    }

            //}

            //if (isAllowUser == 1 || isAllowRoles == 1 || isAllowOrganize == 1)
            //{
            //    return 1;
            //}
            //return isAllowUser;
        }
        protected virtual void Delete(string wildcard)
        {
            DataFactory.Instance().Delete(new Wildcard { WildcardKey = wildcard });
        }
        protected virtual bool Copy(string sourceKey, string destKey)
        {

            var wildcard = DataFactory.Instance().Wildcard(sourceKey);
            if (wildcard != null)
            {
                wildcard.WildcardKey = destKey;
                DataFactory.Instance().Put(wildcard);
            }

            return true;
        }

        /// <summary>
        /// 验证用户是否能通过通配符,0代表没有找到通用，1表示通过，-1表示没有通过
        /// </summary>
        /// <param name="wildcards">通配符</param>
        /// <returns></returns>
        protected virtual int[] Check(params string[] wildcards)
        {

            var vs = new List<int>();
            if (wildcards.Length > 0)
            {
                var authorizes = new List<Wildcard>(DataFactory.Instance().Wildcard(wildcards));//.Query());
                foreach (var w in wildcards)
                {
                    var au = authorizes.Find(a => a.WildcardKey == w);
                    if (au != null)
                    {
                        if (String.IsNullOrEmpty(au.Authorizes))
                        {
                            vs.Add(0);
                        }
                        else
                        {
                            vs.Add(Check(UMC.Data.JSON.Deserialize<UMC.Security.Authorize[]>(au.Authorizes)));

                        }
                    }
                    else
                    {
                        vs.Add(0);
                    }
                }
            }
            return vs.ToArray();

        }
    }
}
