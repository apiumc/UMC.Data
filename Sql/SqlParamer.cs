using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace UMC.Data.Sql
{
    class SqlParamer
    {
        static void AppendDictionary(System.Collections.IDictionary diction, string pfx)
        {

            diction["UMC.True"] = true;
            diction["UMC.False"] = false;
            diction["UMC.DateTime"] = DateTime.Now;
            diction["UMC.Id"] = System.Guid.NewGuid();
            diction["UMC.Prefix"] = pfx;
            diction["UMC.GuidEmpty"] = System.Guid.Empty;
            diction["UMC.GuidNull"] = System.Guid.Empty;
            var user = UMC.Security.Identity.Current;
            if (user == null)
            {
                diction["WebADNuke.Username"] = string.Empty;

            }
            else
            {
                diction["UMC.UserId"] = user.Id;
                diction["UMC.Username"] = user.Name;
            }

        }
        static string[] works = new String[] { "FROM", "JOIN", "UPDATE", "INSERT", "INTO", "DELETE" };
        static string[] endWorks = new String[] { "ON", "WHERE", "ORDER", "GROUP" };
        DbProvider provider;
        DbCommand cmd;
        object[] paramers;
        bool autoPfx;
        string Prefixion = String.Empty;
        bool isFormat = true;

        public static string Format(DbProvider provider, bool autoPfx, DbCommand cmd, String sql, System.Collections.IDictionary diction)
        {
            var parms = new List<object>();
            SqlParamer pfx = new SqlParamer();
            AppendDictionary(diction, provider.Prefixion);
            System.Collections.IDictionaryEnumerator em = diction.GetEnumerator();
            while (em.MoveNext())
            {
                if (em.Key is String)
                {
                    pfx.keys.Add(em.Key as string);
                    parms.Add(em.Value);
                }
            }
            return Format(pfx, provider, autoPfx, cmd, sql, parms.ToArray());
        }
        public static string Format(DbProvider provider, bool autoPfx, DbCommand cmd, String sql, object[] paramers)
        {
            return Format(new SqlParamer(), provider, autoPfx, cmd, sql, paramers);
        }
        /// <summary>
        /// Sql命令进行格式化，把对应的ID={i}转化成对应的ID=?
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="provider">访问管理器</param>
        /// <param name="sql">格式化的Sql文本</param>
        /// <param name="paramers">格式化的参数集</param>
        static string Format(SqlParamer pfx, DbProvider provider, bool autoPfx, DbCommand cmd, String sql, object[] paramers)
        {
            pfx.autoPfx = autoPfx;
            pfx.provider = provider;
            pfx.cmd = cmd;
            pfx.paramers = paramers;
            pfx.paramPfx = "A";
            string prefixion = provider.Prefixion;
            if (String.IsNullOrEmpty(prefixion) == false)
            {
                switch (provider.Delimiter)
                {
                    case ".":
                        pfx.Prefixion = String.Format("{0}{1}{2}.{0}", provider.QuotePrefix, prefixion, provider.QuoteSuffix);
                        break;
                    default:
                        pfx.Prefixion = String.Format("{0}{1}{2}", provider.QuotePrefix, prefixion, provider.Delimiter);
                        break;
                }
            }
            else
            {
                pfx.Prefixion = provider.QuotePrefix;
            }
            return pfx.Do(sql).ToString();
        }
        List<String> keys = new List<string>();
        void Append(StringBuilder sql, object value, string key)
        {
            if (value is Array)
            {
                if (value is byte[])
                {
                    sql.Append(this.provider.AppendDbParameter(key, value, this.cmd));
                }
                else
                {
                    var array = value as Array;
                    var isTop = false;
                    int i = 1;
                    foreach (var o in array)
                    {
                        if (isTop)
                        {
                            sql.Append(',');
                        }
                        isTop = true;
                        sql.Append(this.provider.AppendDbParameter(key + "G" + i, o, this.cmd));
                        i++;
                    }
                }
            }
            else
            {
                sql.Append(this.provider.AppendDbParameter(key, value, this.cmd));
            }
        }


        bool isFrom;
        bool isPrefix;
        string sqlText;
        bool check(String key)
        {
            for (int i = 0; i < works.Length; i++)// (string k in works)
            {
                if (String.Equals(key, works[i], StringComparison.CurrentCultureIgnoreCase))
                {

                    isFrom = i == 0;
                    return true;
                }
            }
            if (this.isFrom)
            {
                foreach (string k in endWorks)
                {
                    if (String.Equals(key, k, StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.isFrom = false;
                        return false;
                    }
                }
            }
            return false;
        }

        void check(int start, int end, StringBuilder sql)
        {
            int b = start;
            if (start > 0)
            {
                start++;

            }
            if (end == 0)
            {
                return;
            }

            if (start > end)
            {
                return;
            }
            var value = sqlText.Substring(start, end - start + 1).Trim();
            if (check(value) == false)
            {
                if (this.isPrefix)
                {
                    switch (value[0])
                    {
                        case '#':
                        case '(':
                            break;
                        default:
                            if (value.IndexOf('.') == -1)
                            {
                                sql.Insert(sql.Length - value.Length, this.Prefixion);
                                //if (value.StartsWith("{pfx}", StringComparison.CurrentCultureIgnoreCase))
                                //{
                                //    sql.Remove(sql.Length - value.Length, 5);
                                //}
                                if (this.isFormat)
                                {
                                    sql.Append(this.provider.QuoteSuffix);
                                }
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(this.Prefixion) == false)
                                {
                                    throw new SqlFormatException(value);
                                }
                            }
                            break;

                    }
                }
                this.isPrefix = false;
            }
            else
            {
                this.isPrefix = true;
            }
        }
        string paramPfx;
        bool check(StringBuilder sb, string value)
        {
            if (this.keys.Count == 0)
            {
                int i = 0;
                if (int.TryParse(value, out i))
                {
                    sb.Remove(sb.Length - 1 - value.Length, value.Length + 1);
                    this.Append(sb, this.paramers[i], paramPfx + value);
                    return true;
                }
            }
            //if (this.autoPfx == false)
            //{
            //    if (String.Equals("pfx", value, StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        sb.Remove(sb.Length - 1 - value.Length, value.Length + 1);
            //        return true;

            //    }
            //}
            if (this.keys.Count > 0)
            {
                //if (String.Equals("pfx", value, StringComparison.CurrentCultureIgnoreCase) == false)
                //{
                int c = this.keys.FindIndex(k => String.Equals(k, value, StringComparison.CurrentCultureIgnoreCase));
                if (c > -1)
                {
                    sb.Remove(sb.Length - 1 - value.Length, value.Length + 1);

                    this.Append(sb, this.paramers[c], paramPfx + c);
                    return true;
                }
                //}
            }
            return false;
        }

        StringBuilder Do(String strSql)
        {
            this.sqlText = strSql;
            var sql = new StringBuilder();
            int i = 0, l = sqlText.Length, start = 0, end = 0, nstart = 0;


            while (i < l)
            {
                char k = sqlText[i];
                switch (k)
                {
                    case '{':
                        end = nstart = i;
                        break;
                    case '}':
                        if (nstart < end)
                        {
                            if (this.check(sql, sqlText.Substring(nstart + 1, end - nstart)))
                            {

                                end = i;
                                i++;
                                continue;
                            }
                        }
                        end = i;
                        break;
                    case '(':

                        if (this.autoPfx)
                        {
                            this.check(start, end, sql);
                            if (this.isFrom)
                            {
                                this.isPrefix = this.isFrom = false;

                            }
                        }
                        end = start = i;
                        break;
                    case ' ':
                    case '\t':
                    case '\b':
                    case '\n':
                    case '\r':
                    case '*':
                    case ')':
                        if (this.autoPfx)
                        {
                            this.check(start, end, sql);
                        }
                        end = start = i;
                        break;
                    case ',':
                        if (this.autoPfx)
                        {
                            if (this.isFrom)
                            {
                                this.check(start, end, sql);
                                this.isPrefix = true;
                                end = start = i;
                            }
                            else
                            {
                                end = i;
                            }
                        }
                        break;
                    default:
                        end = i;
                        break;
                }
                i++;
                sql.Append(k);
            }

            this.check(start, end, sql);
            return sql;

        }
    }
}
