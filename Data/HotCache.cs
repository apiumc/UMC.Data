using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UMC.Net;

namespace UMC.Data
{
    public sealed class HotCache
    {
        static void SynchronizeData()
        {
            SynchronizeValue value;
            while (synchronizes.TryDequeue(out value))
            {
                try
                {
                    var hsh = JSON.Deserialize<Hashtable>(value.Value);
                    if (hsh != null)
                    {
                        var p = WebResource.Instance();
                        String appId = p.Provider["appId"];
                        var point = hsh["point"] as string;


                        if (String.Equals(point, SyncPoint) == false)
                        {

                            var time = hsh["time"] as string;
                            var type = hsh["type"] as string;

                            System.Collections.Specialized.NameValueCollection nvs = new System.Collections.Specialized.NameValueCollection();

                            nvs.Add("from", appId);
                            nvs.Add("time", time);
                            nvs.Add("point", point);
                            nvs.Add("type", type);
                            var secret = p.Provider["appSecret"];

                            if (String.Equals(hsh["sign"] as string, UMC.Data.Utility.Sign(nvs, secret)))
                            {
                                var m = dictionary.GetEnumerator();
                                while (m.MoveNext())
                                {
                                    if (String.Equals(m.Current.Key.FullName, type))
                                    {
                                        ISynchronize synchronize = (ISynchronize)m.Current.Value;
                                        synchronize.SyncData(hsh, value.ip);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    UMC.Data.Utility.Error("Synchronize", DateTime.Now, ex.ToString());
                }
            }
        }
        public static Uri Uri
        {
            get
            {
                if (_apiUrl == null)
                {
                    var appId = WebResource.Instance().Provider["appId"];
                    if (String.IsNullOrEmpty(appId) == false)
                    {
                        String code = Utility.Parse36Encode(Utility.Guid(appId).Value);
                        _apiUrl = new Uri(String.Format("https://api.365lu.cn/{0}/", code));
                    }
                    else
                    {
                        throw new ArgumentException("缺少appId参数", "HotCache");
                    }
                }
                return _apiUrl;
            }
        }
        static Uri _apiUrl;


        static Dictionary<Type, Object> dictionary = new Dictionary<Type, Object>();
        public static Cache<T> Register<T>(params String[] primaryKeys) where T : class
        {
            var hHotCache = new Cache<T>(new DbPersistent<T>("defaultDbProvider"), primaryKeys);
            dictionary[typeof(T)] = hHotCache;
            return hHotCache;
        }
        public static Cache<T> Register<T>(String providerName, String[] primaryKeys) where T : class
        {
            var hHotCache = new Cache<T>(new DbPersistent<T>(providerName), primaryKeys);
            dictionary[typeof(T)] = hHotCache;
            return hHotCache;
        }
        public static Cache<T> MemoryRegister<T>(params String[] primaryKeys) where T : class
        {
            var hHotCache = new Cache<T>(new Memory<T>(false), primaryKeys);
            dictionary[typeof(T)] = hHotCache;
            return hHotCache;
        }
        public static Cache<T> ObjectRegister<T>(params String[] primaryKeys) where T : class
        {
            var hHotCache = new Cache<T>(new Memory<T>(true), primaryKeys);
            dictionary[typeof(T)] = hHotCache;
            return hHotCache;
        }
        public static Cache<T> NetDBRegister<T>(params String[] primaryKeys) where T : class
        {
            var hHotCache = new Cache<T>(new NetDBPersistent<T>(new DbPersistent<T>("defaultDbProvider")), primaryKeys);
            dictionary[typeof(T)] = hHotCache;
            return hHotCache;
        }
        public static Cache<T> NetRegister<T>(params String[] primaryKeys) where T : class
        {
            var hHotCache = new Cache<T>(new NetPersistent<T>(), primaryKeys);
            dictionary[typeof(T)] = hHotCache;
            return hHotCache;
        }
        public static void Clear()
        {
            var m = dictionary.GetEnumerator();
            while (m.MoveNext())
            {
                ISynchronize synchronize = (ISynchronize)m.Current.Value;
                synchronize.Clear();
            }

        }
        public static void Clear(String type)
        {
            var m = dictionary.GetEnumerator();
            while (m.MoveNext())
            {
                if (String.Equals(m.Current.Key.FullName, type))
                {
                    ISynchronize synchronize = (ISynchronize)m.Current.Value;
                    synchronize.Clear();
                }
            }

        }
        public static System.Data.DataTable Caches()
        {
            var data = new System.Data.DataTable();
            data.Columns.Add("full");
            data.Columns.Add("name");
            data.Columns.Add("size");
            var m = dictionary.GetEnumerator();
            while (m.MoveNext())
            {
                ISynchronize synchronize = (ISynchronize)m.Current.Value;
                //synchronize.Clear();
                data.Rows.Add(m.Current.Key.FullName, m.Current.Key.Name, synchronize.Length);
            }
            return data;

        }
        class SynchronizeValue
        {
            public String Value;
            public String ip;
        }
        static ConcurrentQueue<SynchronizeValue> synchronizes = new ConcurrentQueue<SynchronizeValue>();
        public static void Synchronize(String value, String ip)
        {
            if (synchronizes.IsEmpty)
            {
                synchronizes.Enqueue(new SynchronizeValue { ip = ip, Value = value });
                System.Threading.Tasks.Task.Factory.StartNew(SynchronizeData);
            }
            else
            {
                synchronizes.Enqueue(new SynchronizeValue { ip = ip, Value = value });
            }


        }
        public static Object Cache(String type, Hashtable value)
        {
            var m = dictionary.GetEnumerator();
            while (m.MoveNext())
            {
                if (String.Equals(m.Current.Key.FullName, type))
                {
                    ISynchronize synchronize = (ISynchronize)m.Current.Value;
                    return synchronize.Cache(value);

                }
            }
            return null;
        }


        static readonly String SyncPoint = UMC.Data.Utility.IntParse(Guid.NewGuid()).ToString();
        public static void Synchronize<T>(string[] fields, object[] values)
        {
            var newObject = new Hashtable();
            for (var i = 0; i < fields.Length; i++)
            {
                newObject[fields[i]] = values[i];

            }
            var p = WebResource.Instance();
            String appId = p.Provider["appId"];

            if (String.IsNullOrEmpty(appId) == false)
            {
                var secret = p.Provider["appSecret"];
                var type = typeof(T).FullName;
                var nvs = new System.Collections.Specialized.NameValueCollection();
                var time = UMC.Data.Utility.TimeSpan().ToString();
                nvs.Add("from", appId);
                nvs.Add("time", time);
                nvs.Add("point", SyncPoint);
                nvs.Add("type", type);

                var webD = new Web.WebMeta();
                webD.Put("from", appId);
                webD.Put("time", time);
                webD.Put("point", SyncPoint);
                webD.Put("type", type);
                webD.Put("sign", UMC.Data.Utility.Sign(nvs, secret));
                webD.Put("value", newObject);

                DataFactory.Instance().SynchData(0x01, UMC.Data.JSON.Serialize(webD));
            }

        }

        public static void Remove<T>(T obj) where T : class
        {
            var type = typeof(T);
            object cache;
            if (dictionary.TryGetValue(type, out cache))
            {
                var ca = (Cache<T>)cache;
                ca.Remove(obj);
            }


        }

        public static Cache<T> Cache<T>() where T : class
        {
            var type = typeof(T);
            object cache;
            if (dictionary.TryGetValue(type, out cache))
            {
                return (Cache<T>)cache;
            }
            return null;

        }
        internal interface IPersistent<T> where T : class
        {
            void Put(String[] fields, T[] values);
            T Put(String[] fields, Object[] values, T value);
            void Delete(String[] fields, Object[] values);
            T Get(String[] fields, Object[] values);
            T[] Get(T search, String field, params Object[] values);
            T[] Get(T search, int start, int limit, out int total);

            bool IsPersistent();
        }
        internal interface ISynchronize
        {
            object Cache(System.Collections.Hashtable value);
            void SyncData(System.Collections.Hashtable value, String ip);
            void Clear();
            int Length { get; }
        }
        class DbPersistent<T> : IPersistent<T> where T : class
        {
            public bool IsPersistent()
            {
                return true;
            }
            String providerName;
            public DbPersistent(String dbName)
            {
                providerName = dbName;
            }
            public void Delete(string[] fields, object[] values)
            {
                if (fields.Length > 0 && fields.Length == values.Length)
                {
                    var wh = UMC.Data.Database.Instance(providerName).ObjectEntity<T>();
                    for (var i = 0; i < fields.Length; i++)
                    {
                        wh.Where.And().Equal(fields[i], values[i]);
                    }
                    wh.Delete();

                    HotCache.Synchronize<T>(fields, values);
                }

            }
            public void Put(string[] fields, T[] values)
            {
                if (fields.Length > 0)
                {
                    var property = typeof(T).GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance);

                    var wh = UMC.Data.Database.Instance(providerName).ObjectEntity<T>();
                    foreach (var v in values)
                    {
                        wh.Where.Reset();

                        for (var i = 0; i < fields.Length; i++)
                        {
                            var p = property.First(r => r.Name == fields[i]);
                            var value = p.GetValue(v);
                            if (value != null)
                            {
                                wh.Where.And().Equal(fields[i], value);

                            }
                        }
                        if (wh.Update(v) == 0)
                        {
                            wh.Insert(v);
                        }
                    }


                }

            }


            public T Get(string[] fields, object[] values)
            {

                var wh = UMC.Data.Database.Instance(providerName).ObjectEntity<T>();
                for (var i = 0; i < fields.Length; i++)
                {
                    wh.Where.And().Equal(fields[i], values[i]);
                }
                return wh.Single();
            }

            public T[] Get(T search, string field, params object[] values)
            {

                var wh = UMC.Data.Database.Instance(providerName).ObjectEntity<T>()
                  .Where.And().Equal(search);
                if (values.Length > 0)
                {
                    wh.And().In(field, values);
                }
                return wh.Entities.Query();

            }


            public T[] Get(T search, int start, int limit, out int total)
            {


                var wh = UMC.Data.Database.Instance(providerName).ObjectEntity<T>().Where.And().Equal(search).Entities;
                total = wh.Count();
                return wh.Query(start, limit);


            }

            public T Put(string[] fields, object[] values, T value)
            {
                if (fields.Length > 0 && fields.Length == values.Length)
                {
                    HotCache.Synchronize<T>(fields, values);

                    var wh = UMC.Data.Database.Instance(providerName).ObjectEntity<T>();
                    for (var i = 0; i < fields.Length; i++)
                    {
                        wh.Where.And().Equal(fields[i], values[i]);
                    }
                    if (wh.Update(value) > 0)
                    {
                        HotCache.Synchronize<T>(fields, values);
                        return wh.Single();
                    }
                    else
                    {
                        wh.Insert(value);
                        HotCache.Synchronize<T>(fields, values);
                    }
                    return value;


                }
                return null;
            }
        }

        public static void Sign(System.Net.HttpWebRequest http, System.Collections.Specialized.NameValueCollection nvs, String secret)
        {
            nvs.Add("umc-client-pfm", "sync");
            nvs.Add("umc-request-time", UMC.Data.Utility.TimeSpan().ToString());
            nvs.Add("umc-request-sign", UMC.Data.Utility.Sign(nvs, secret));


            for (var i = 0; i < nvs.Count; i++)
            {
                http.Headers.Add(nvs.GetKey(i), nvs[i]);
            }
        }
        class Memory<T> : IPersistent<T> where T : class
        {
            ConcurrentDictionary<String, T> keyValues = new ConcurrentDictionary<String, T>();
            public void Delete(string[] fields, object[] values)
            {
                if (_isPersistent == false)
                {
                    T o;
                    keyValues.TryRemove(String.Join(",", values), out o);
                }
                HotCache.Synchronize<T>(fields, values);
            }
            public Memory(bool isObject)
            {
                this._isPersistent = !isObject;
            }
            bool _isPersistent;
            public bool IsPersistent()
            {
                return _isPersistent;
            }
            public T Get(string[] fields, object[] values)
            {
                if (_isPersistent)
                {
                    return null;
                }
                T v;
                keyValues.TryGetValue(String.Join(",", values), out v);
                return v;
            }

            public T[] Get(T search, string field, params object[] values)
            {
                return new T[0];
            }

            public T[] Get(T search, int start, int limit, out int total)
            {
                total = 0;

                return new T[0];
            }

            public void Put(string[] fields, T[] values)
            {

            }

            public T Put(string[] fields, object[] values, T value)
            {
                if (_isPersistent == false)
                {
                    keyValues[String.Join(",", values)] = value;
                }
                HotCache.Synchronize<T>(fields, values);
                return value;

            }
        }
        class NetDBPersistent<T> : IPersistent<T> where T : class
        {
            DbPersistent<T> dbPersistent;
            public NetDBPersistent(DbPersistent<T> persistent)
            {
                dbPersistent = persistent;
            }
            public bool IsPersistent()
            {
                return true;
            }
            public Uri Uri
            {
                get
                {
                    return HotCache.Uri;
                }
            }
            public void Delete(string[] fields, object[] values)
            {
                var keyPath = new StringBuilder();
                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");
                for (int i = 0; i < fields.Length; i++)
                {
                    if (i > 0)
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(fields[i]));
                    keyPath.Append("=");

                    keyPath.Append(System.Uri.EscapeUriString(values[i].ToString()));
                }

                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);


                http.Delete(r =>
                {
                    if (r.StatusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            dbPersistent.Delete(fields, values);
                        }
                        catch (Exception ex)
                        {
                            UMC.Data.Utility.Error("HotCache", DateTime.Now, ex.ToString());
                        }
                    }
                });


            }

            void Sign(System.Collections.Specialized.NameValueCollection nvs, System.Net.HttpWebRequest http)
            {
                var secret = WebResource.Instance().Provider["appSecret"];
                if (String.IsNullOrEmpty(secret) == false)
                {

                    HotCache.Sign(http, nvs, secret);
                }
            }

            public T Get(string[] fields, object[] values)
            {

                return dbPersistent.Get(fields, values);
            }


            public T[] Get(T field, int start, int limit, out int total)
            {
                return dbPersistent.Get(field, start, limit, out total);

            }

            public T[] Get(T search, string field, params object[] values)
            {
                var ts = dbPersistent.Get(search, field, values);
                if (values.Length > 0 || ts.Length > 0)
                {
                    return ts;
                }
                var secret = WebResource.Instance().Provider["appSecret"];
                if (String.IsNullOrEmpty(secret))
                {
                    return new T[0];
                }

                var keyPath = new StringBuilder();
                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");
                bool isOne = true;
                var vs = new List<Object>(values);

                var fvalue = UMC.Data.Reflection.PropertyToDictionary(search);
                if (fvalue.Count > 0)
                {
                    var em = fvalue.GetEnumerator();
                    while (em.MoveNext())
                    {
                        var key = em.Key as string;
                        if (String.Equals(key, field))
                        {
                            if (vs.Exists(r => r.Equals(em.Value)) == false)
                            {
                                vs.Add(em.Value);
                            }
                        }
                        else
                        {
                            if (isOne)
                            {
                                isOne = false;
                            }
                            else
                            {
                                keyPath.Append("&");
                            }
                            keyPath.Append(System.Uri.EscapeUriString(key));
                            keyPath.Append("=");
                            keyPath.Append(System.Uri.EscapeUriString(em.Value.ToString()));
                        }
                    }
                }



                if (vs.Count > 0)
                {
                    if (isOne)
                    {
                        isOne = false;
                    }
                    else
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(field));
                    keyPath.Append("=");
                    for (int i = 0; i < vs.Count; i++)
                    {
                        if (i > 0)
                        {
                            keyPath.Append(",");
                        }
                        keyPath.Append(System.Uri.EscapeUriString(vs[i].ToString()));
                    }
                }

                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                nvs.Add("umc-sync-type", "array");

                this.Sign(nvs, http);
                var httpResponse = http.Get();
                var text = httpResponse.ReadAsString();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var tvs = UMC.Data.JSON.Deserialize<T[]>(text);
                    if (tvs.Length > 0)
                    {
                        dbPersistent.Put(Cache<T>().MainKey, tvs);
                    }
                    return tvs;
                }
                return new T[0];


            }
            public T Put(string[] fields, object[] values, T value)
            {

                var keyPath = new StringBuilder();

                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");

                for (int i = 0; i < fields.Length; i++)
                {
                    if (i > 0)
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(fields[i]));
                    keyPath.Append("=");
                    keyPath.Append(System.Uri.EscapeUriString(values[i].ToString()));
                }
                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);


                http.Put(value, httpResponse =>
                {
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        try
                        {
                            dbPersistent.Put(fields, values, value);
                        }
                        catch (Exception ex)
                        {
                            UMC.Data.Utility.Error("HotCache", DateTime.Now, ex.ToString());
                        }
                    }
                });
                return value;

            }

            public void Put(string[] fields, T[] values)
            {

                var keyPath = new StringBuilder();

                keyPath.Append(typeof(T).Name);
                if (fields.Length > 0)
                {
                    keyPath.Append("?fields=");
                    keyPath.Append(System.Uri.EscapeUriString(String.Join(",", fields)));
                }
                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);

                http.Put(values, res =>
                {
                    if (res.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        try
                        {
                            dbPersistent.Put(fields, values);
                        }
                        catch (Exception ex)
                        {
                            UMC.Data.Utility.Error("HotCache", DateTime.Now, ex.ToString());
                        }
                    }

                });//.ReadAsString();//.Start();


            }
        }
        class NetPersistent<T> : IPersistent<T> where T : class
        {
            public bool IsPersistent()
            {
                return true;
            }
            public Uri Uri
            {
                get
                {
                    return HotCache.Uri;
                }
            }
            public void Delete(string[] fields, object[] values)
            {

                var keyPath = new StringBuilder();
                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");
                for (int i = 0; i < fields.Length; i++)
                {
                    if (i > 0)
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(fields[i]));
                    keyPath.Append("=");

                    keyPath.Append(System.Uri.EscapeUriString(values[i].ToString()));
                }

                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);


                http.Delete().ReadAsString();
                HotCache.Synchronize<T>(fields, values);


            }

            void Sign(System.Collections.Specialized.NameValueCollection nvs, System.Net.HttpWebRequest http)
            {
                var secret = WebResource.Instance().Provider["appSecret"];
                if (String.IsNullOrEmpty(secret) == false)
                {
                    HotCache.Sign(http, nvs, secret);
                }
            }

            public T Get(string[] fields, object[] values)
            {

                var keyPath = new StringBuilder();
                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");
                for (int i = 0; i < fields.Length; i++)
                {
                    if (i > 0)
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(fields[i]));
                    keyPath.Append("=");
                    keyPath.Append(System.Uri.EscapeUriString(values[i].ToString()));
                }

                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);
                var httpResponse = http.Get();


                var text = httpResponse.ReadAsString();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK && String.IsNullOrEmpty(text) == false)
                {
                    return UMC.Data.JSON.Deserialize<T>(text);
                    
                }
                return null;

            }


            public T[] Get(T field, int start, int limit, out int total)
            {
                var secret = WebResource.Instance().Provider["appSecret"];
                if (String.IsNullOrEmpty(secret))
                {
                    total = 0;
                    return new T[0];
                }

                var keyPath = new StringBuilder();

                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");
                bool isOne = true;
                var fvalue = UMC.Data.Reflection.PropertyToDictionary(field);
                if (fvalue.Count > 0)
                {
                    var em = fvalue.GetEnumerator();
                    while (em.MoveNext())
                    {
                        if (isOne)
                        {
                            isOne = false;
                        }
                        else
                        {
                            keyPath.Append("&");
                        }
                        keyPath.Append(System.Uri.EscapeUriString(em.Key.ToString()));
                        keyPath.Append("=");
                        keyPath.Append(System.Uri.EscapeUriString(em.Value.ToString()));
                    }
                }
                if (isOne == false)
                {
                    keyPath.Append("&");
                }
                keyPath.AppendFormat("start={0}&limit={1}", start, limit);

                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                nvs.Add("umc-sync-type", "page");
                nvs.Add("umc-sync-start", start.ToString());
                nvs.Add("umc-sync-limit", limit.ToString());
                this.Sign(nvs, http);


                var httpResponse = http.Get();

                total = UMC.Data.Utility.IntParse(httpResponse.Headers.Get("umc-sync-count"), 0);
                var text = httpResponse.ReadAsString();

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return UMC.Data.JSON.Deserialize<T[]>(text);
               
                }
                return new T[0];

            }

            public T[] Get(T search, string field, params object[] values)
            {
                var secret = WebResource.Instance().Provider["appSecret"];
                if (String.IsNullOrEmpty(secret))
                {
                    return new T[0];
                }

                var keyPath = new StringBuilder();
                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");
                bool isOne = true;
                var vs = new List<Object>(values);

                var fvalue = UMC.Data.Reflection.PropertyToDictionary(search);
                if (fvalue.Count > 0)
                {
                    var em = fvalue.GetEnumerator();
                    while (em.MoveNext())
                    {
                        var key = em.Key as string;
                        if (String.Equals(key, field))
                        {
                            if (vs.Exists(r => r.Equals(em.Value)) == false)
                            {
                                vs.Add(em.Value);
                            }
                        }
                        else
                        {
                            if (isOne)
                            {
                                isOne = false;
                            }
                            else
                            {
                                keyPath.Append("&");
                            }
                            keyPath.Append(System.Uri.EscapeUriString(key));
                            keyPath.Append("=");
                            keyPath.Append(System.Uri.EscapeUriString(em.Value.ToString()));
                        }
                    }
                }



                if (vs.Count > 0)
                {
                    if (isOne)
                    {
                        isOne = false;
                    }
                    else
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(field));
                    keyPath.Append("=");
                    for (int i = 0; i < vs.Count; i++)
                    {
                        if (i > 0)
                        {
                            keyPath.Append(",");
                        }
                        keyPath.Append(System.Uri.EscapeUriString(vs[i].ToString()));
                    }
                }

                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                nvs.Add("umc-sync-type", "array");

                this.Sign(nvs, http);


                var httpResponse = http.Get();
                var text = httpResponse.ReadAsString();
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return UMC.Data.JSON.Deserialize<T[]>(text);
               
                }
                return new T[0];


            }
            public T Put(string[] fields, object[] values, T value)
            {

                var keyPath = new StringBuilder();

                keyPath.Append(typeof(T).Name);
                keyPath.Append("?");

                for (int i = 0; i < fields.Length; i++)
                {
                    if (i > 0)
                    {
                        keyPath.Append("&");
                    }
                    keyPath.Append(System.Uri.EscapeUriString(fields[i]));
                    keyPath.Append("=");
                    keyPath.Append(System.Uri.EscapeUriString(values[i].ToString()));
                }
                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);


                http.Put(value, res =>
                  {
                      if (res.StatusCode == System.Net.HttpStatusCode.OK)
                      {
                          HotCache.Synchronize<T>(fields, values);
                      
                      }
                  });
                return value;

            }

            public void Put(string[] fields, T[] values)
            {

                var keyPath = new StringBuilder();

                keyPath.Append(typeof(T).Name);
                if (fields.Length > 0)
                {
                    keyPath.Append("?fields=");
                    keyPath.Append(System.Uri.EscapeUriString(String.Join(",", fields)));
                }
                var http = new Uri(this.Uri, keyPath.ToString()).WebRequest();
                var nvs = new System.Collections.Specialized.NameValueCollection();
                this.Sign(nvs, http);


                http.Put(values, res =>
                { 
                });


            }
        }
    }
}
