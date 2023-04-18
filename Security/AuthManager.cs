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
        static AuthManager _instance;
        public static void Instance(AuthManager authManager)
        {
            _instance = authManager;
        }
        public static AuthManager Instance()
        {
            if (_instance == null)
            {
                _instance = new AuthManager();
            }
            return _instance;
        }
        /// <summary>
        /// 批量验证权限
        /// </summary>
        /// <param name="wildcards"></param>
        /// <returns></returns>
        public static bool[] IsAuthorization(Identity user, int site, params string[] wildcards)
        {
            bool[] rerValue = new bool[wildcards.Length];
            if (wildcards.Length > 0)
            {
                if (user.IsInRole(UMC.Security.Membership.AdminRole))
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
                var wMger = Instance();// new AuthManager();
                var vs = wMger.Check(user, site, list.ToArray());
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
        /// 验证用户是否能通过通配符
        /// </summary>
        /// <param name="user">用户身份</param>
        /// <param name="site">通配符</param>
        /// <param name="wildcard">通配符</param>
        /// <returns></returns>
        public static bool IsAuthorization(Identity user, int site, string wildcard)
        {

            return IsAuthorization(user, site, new string[] { wildcard })[0];

        }

        public static List<Tuple<byte, String>> Authorize(byte[] authorizes)
        {
            var lis = new List<Tuple<byte, String>>();
            if (authorizes != null)
            {
                var start = 0;
                for (var i = 0; i < authorizes.Length; i++)
                {
                    if (i == authorizes.Length - 1)
                    {
                        lis.Add(Tuple.Create(authorizes[start], System.Text.Encoding.UTF8.GetString(authorizes, start + 1, i - start)));

                    }
                    else if (authorizes[i] == 13)
                    {
                        lis.Add(Tuple.Create(authorizes[start], System.Text.Encoding.UTF8.GetString(authorizes, start + 1, i - start - 1)));
                    }
                    else
                    {
                        continue;
                    }

                    start = i + 1;

                }
            }
            return lis;//.ToArray();

        }
        public static byte[] Authorize(Tuple<byte, String>[] authorizes)
        {
            var lis = new List<byte>();
            for (var i = 0; i < authorizes.Length; i++)
            {
                if (i > 0)
                {
                    lis.Add(13);
                }
                lis.Add(authorizes[i].Item1);
                lis.AddRange(System.Text.Encoding.UTF8.GetBytes(authorizes[i].Item2));
            }
            return lis.ToArray();

        }


        int Check(Identity user, byte[] authorizes)
        {
            int isAllowRoles = 0;
            int isAllowUser = 0;
            int isAllowOrganize = 0;
            var username = user.Name;
            if (authorizes == null)
            {
                return 0;
            }
            var start = 0;
            for (var i = 0; i < authorizes.Length; i++)
            {
                int l = 0;
                if (i == authorizes.Length - 1)
                {
                    l = i - start;
                }
                else if (authorizes[i] == 13)
                {
                    l = i - start - 1;
                }
                else
                {
                    continue;
                }

                var sValue = System.Text.Encoding.UTF8.GetString(authorizes, start + 1, l);
                switch (authorizes[start])
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
                                if (user.IsAuthenticated == false)
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
                            if (user.IsInRole(sValue))
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
                                    if (user.IsAuthenticated == false)
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

                            if (user.IsOrganizeMember(sValue))
                            {
                                isAllowOrganize = 1;
                            }
                            else
                            {
                                isAllowOrganize = -1;
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

                            if (user.IsOrganizeMember(sValue))
                            {
                                isAllowOrganize = -1;
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
                        else if (isAllowRoles != -1 && user.IsInRole(sValue))
                        {
                            isAllowRoles = -1;

                        }
                        break;
                }
                start = i + 1;

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
        }

        /// <summary>
        /// 验证用户是否能通过通配符,0代表没有找到通用，1表示通过，-1表示没有通过
        /// </summary>
        /// <param name="wildcards">通配符</param>
        /// <returns></returns>
        public virtual int[] Check(Identity user, int site, params string[] wildcards)
        {

            var vs = new List<int>();
            if (wildcards.Length > 0)
            {
                var authorizes = new List<Authority>(DataFactory.Instance().Authority(site, wildcards));
                foreach (var w in wildcards)
                {
                    var au = authorizes.Find(a => a.Key == w);
                    if (au != null)
                    {
                        if (au.Body == null || au.Body.Length == 0)
                        {
                            vs.Add(0);
                        }
                        else
                        {
                            vs.Add(Check(user, au.Body));
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
