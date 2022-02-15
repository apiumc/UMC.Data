using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace UMC.Data.Sql
{
    class CBO
    {
        public static IDictionary<string, object> GetProperty(object obj)
        {
            var dic = new Dictionary<string, object>();
            if (obj != null)
            {
                if (obj == null) obj = DBNull.Value;
                var cache = obj.GetType().GetProperties();
                foreach (PropertyInfo info in cache)
                {
                    if (info.CanRead)
                    {
                        var value = info.GetValue(obj, null) ?? null;
                        var pType = info.PropertyType;
                        if (value != null)
                        {
                            dic.Add(info.Name, value);
                        }
                    }

                }
            }

            return dic;
        }
        internal static T CreateObject<T>(EntityHelper sqltext, IDataReader dr)
        {
            T tObj = Activator.CreateInstance<T>();
            return (T)CreateObject(tObj, sqltext, dr);
        }
        internal static readonly Type Nullable = typeof(Nullable<>);
        internal static Object CreateObject(object obvalue, EntityHelper sqltext, IDataReader dr)
        {
            return sqltext.CreateObject(obvalue, dr);
        }
    }



}