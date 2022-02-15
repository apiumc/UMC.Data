using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UMC.Net;

namespace UMC.Data
{
    public sealed class Cache<T> : HotCache.ISynchronize where T : class
    {

        class CacheValue
        {
            Object[][] _values = new object[1][];
            int _size = 0;
            public int Count
            {
                get
                {
                    return _size;
                }
            }
            public Object[][] Rows
            {
                get
                {
                    var v = new object[_size][];
                    Array.Copy(_values, v, _size);
                    return v;
                }
            }
            public List<Object[]> Get(int[] indexs, object[] vls)
            {
                List<Object[]> vs = new List<object[]>();
                for (var i = 0; i < _size; i++)
                {
                    var k = _values[i];
                    if (Check(indexs, vls, k))
                    {
                        vs.Add(k);
                    }
                }
                return vs;
            }
            bool Check(int[] indexs, object[] vls, object[] rowValue)
            {
                for (var i = 0; i < vls.Length; i++)
                {
                    if (Object.Equals(vls[i], rowValue[indexs[i]]) == false)
                    {
                        return false;
                    }

                }
                return true;
            }
            public void Put(int[] indexs, object[] value)
            {
                lock (this)
                {
                    object[] kva = new object[indexs.Length];
                    for (var i = 0; i < indexs.Length; i++)
                    {
                        kva[i] = value[indexs[i]];
                    }
                    for (var i = 0; i < _size; i++)
                    {
                        var val = _values[i];
                        if (Check(indexs, kva, val))
                        {

                            for (int c = 0; c < value.Length; c++)
                            {
                                var oc = value[c];
                                if (oc != null)
                                {
                                    val[c] = oc;
                                }
                            }
                            return;
                        }
                    }
                    if (_size >= _values.Length)
                    {
                        var v = new object[_values.Length + 1][];
                        Array.Copy(_values, v, _size);
                        _values = v;

                    }
                    _values[_size] = value;
                    _size++;
                }


            }
            public Object[] Remove(int[] indexs, object[] value)
            {
                lock (this)
                {
                    for (var i = 0; i < _size; i++)
                    {
                        var v = _values[i];
                        if (Check(indexs, value, v))
                        {
                            _size--;
                            Array.Copy(_values, i + 1, _values, i, _size - i);
                            return v;
                        }
                    }
                }
                return null;

            }

        }

        HotCache.IPersistent<T> persistent;

        PropertyInfo[] properties;
        List<int[]> MKeys = new List<int[]>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persistent"></param>
        /// <param name="primaryKeys"></param>
        internal Cache(HotCache.IPersistent<T> persistent, params String[] primaryKeys)
        {
            this.persistent = persistent;

            this.properties = typeof(T).GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance).Where(r => r.CanRead && r.CanWrite).ToArray();
            var keys = new List<int>();
            foreach (var k in primaryKeys)
            {
                for (var i = 0; i < this.properties.Length; i++)
                {
                    if (properties[i].Name == k)
                    {
                        keys.Add(i);
                        break;
                    }
                }
            }
            if (keys.Count > 0)
            {
                this.MKeys.Add(keys.ToArray());
            }

        }
        /// <summary>
        /// 注册唯一健
        /// </summary>
        /// <param name="indexKeys"></param>
        /// <returns></returns>
        public Cache<T> Register(params String[] indexKeys)
        {
            if (indexKeys.Length > 0)
            {
                var keys = new List<int>();
                foreach (var k in indexKeys)
                {
                    for (var i = 0; i < this.properties.Length; i++)
                    {
                        if (properties[i].Name == k)
                        {
                            keys.Add(i);
                            break;
                        }
                    }
                }
                if (keys.Count > 0)
                {
                    this.MKeys.Add(keys.ToArray());
                    this.indexKeys.Add(new CacheSet<ulong, CacheValue>());
                }
            }
            return this;
        }
        CacheSet<ulong, CacheValue> dataCache = new CacheSet<ulong, CacheValue>();

        List<CacheSet<ulong, CacheValue>> indexKeys = new List<CacheSet<ulong, CacheValue>>();

        Object[] Split(T vale)
        {
            var ls = new List<object>();
            foreach (var p in this.properties)
            {
                ls.Add(p.GetValue(vale, null));

            }
            return ls.ToArray();
        }

        public void Put(T vale)
        {
            var value = Split(vale);
            var values = new List<Object>();
            int keyIndex;
            var mainKey = GetKey(value, out keyIndex, values);
            if (mainKey != 0 && keyIndex == 0)
            {

                if (this.persistent.IsPersistent())
                {
                    if (IsPutOnly)
                    {
                        this.Put(this.GetFeilds(MKeys[0]), values.ToArray(), vale);
                    }
                    else
                    {
                        CacheValue sortedValue;
                        if (dataCache.TryGetValue(mainKey, out sortedValue))
                        {
                            var kvs = sortedValue.Get(this.MKeys[0], values.ToArray());
                            if (kvs.Count > 0)
                            {

                                this.RemoveIndex(kvs[0]);
                                sortedValue.Put(this.MKeys[0], value);
                                RegisterIndex(kvs[0], sortedValue);
                            }
                            else
                            {
                                sortedValue.Put(this.MKeys[0], value);
                                RegisterIndex(value, sortedValue);
                            }

                        }
                        else
                        {
                            sortedValue = new CacheValue();
                            sortedValue.Put(this.MKeys[0], value);
                            dataCache.Put(mainKey, sortedValue);
                            RegisterIndex(value, sortedValue);
                        }


                        this.Put(this.GetFeilds(MKeys[0]), values.ToArray(), vale);

                    }
                }
                else
                {
                    this.persistent.Put(this.GetFeilds(MKeys[0]), values.ToArray(), vale);
                }
            }


        }

        void Delete(String[] fields, Object[] values)
        {

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.persistent.Delete(fields, values);
            });
        }
        void Put(String[] fields, Object[] values, T value)
        {

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var v = this.persistent.Put(fields, values, value);
                if (v != null && Object.ReferenceEquals(value, v) == false)
                {
                    this.Register(this.Split(v));
                }

            });

        }

        T[] Clone(List<Object[]> values)
        {

            List<T> ts = new List<T>();


            foreach (var vs in values)
            {
                T newObject = Activator.CreateInstance<T>();
                for (var i = 0; i < vs.Length; i++)
                {
                    var p = this.properties[i];

                    object propertyvalue = vs[i];
                    if (propertyvalue != null)
                    {
                        p.SetValue(newObject, propertyvalue, null);
                    }
                }
                ts.Add(newObject);
            }

            return ts.ToArray();
        }
        public bool IsPutOnly
        {
            get; set;
        }
        public bool IsSyncData
        {
            get; set;
        }

        public void Remove(T t)
        {
            this.Delete(t, false);

        }
        T GetCache(T t)
        {

            var values = new List<Object>();
            int keyIndex;
            var key = GetKey(Split(t), out keyIndex, values);
            if (key != 0)
            {
                if (keyIndex == 0)
                {
                    CacheValue sortedValue;
                    if (dataCache.TryGetValue(key, out sortedValue))
                    {
                        var rvalues = sortedValue.Get(MKeys[keyIndex], values.ToArray());
                        if (rvalues.Count > 0)
                        {
                            return Clone(rvalues)[0];

                        }
                    }

                }
                else
                {
                    var indexSet = indexKeys[keyIndex - 1];

                    CacheValue sortedValue;

                    if (indexSet.TryGetValue(key, out sortedValue))
                    {


                        var rvalues = sortedValue.Get(MKeys[keyIndex], values.ToArray());
                        if (rvalues.Count > 0)
                        {
                            return Clone(rvalues)[0];

                        }
                    }
                }

            }
            return null;
        }
        public bool IsFast { get; set; }
        public T Get(T t)
        {
            var values = new List<Object>();
            int keyIndex;
            var key = GetKey(Split(t), out keyIndex, values);
            if (key > 0)
            {
                if (keyIndex == 0)
                {

                    if (this.persistent.IsPersistent() == false)
                    {
                        return this.persistent.Get(GetFeilds(this.MKeys[0]), values.ToArray());

                    }
                    CacheValue sortedValue;
                    if (dataCache.TryGetValue(key, out sortedValue))
                    {
                        var rvalues = sortedValue.Get(MKeys[keyIndex], values.ToArray());
                        if (rvalues.Count > 0)
                        {
                            return Clone(rvalues)[0];

                        }
                    }
                    if (IsFast)
                    {
                        if (dataCache.Count == 0 && isSearchAll == false)
                        {
                            isSearchAll = true;
                            var search = this.persistent.Get(Activator.CreateInstance<T>(), "", new object[0]);
                            foreach (var k in search)
                            {
                                Register(Split(k));
                            }
                            if (dataCache.TryGetValue(key, out sortedValue))
                            {
                                var rvalues = sortedValue.Get(MKeys[keyIndex], values.ToArray());
                                if (rvalues.Count > 0)
                                {
                                    return Clone(rvalues)[0];

                                }
                            }
                        }
                        return default(T);
                    }
                }
                else
                {
                    var indexSet = indexKeys[keyIndex - 1];
                    CacheValue sortedValue;
                    if (indexSet.TryGetValue(key, out sortedValue))
                    {
                        var rvalues = sortedValue.Get(MKeys[keyIndex], values.ToArray());
                        if (rvalues.Count > 0)
                        {
                            return Clone(rvalues)[0];
                        }
                    }
                    if (IsFast)
                    {
                        if (dataCache.Count == 0 && isSearchAll == false)
                        {
                            isSearchAll = true;
                            var search = this.persistent.Get(Activator.CreateInstance<T>(), "", new object[0]);
                            foreach (var k in search)
                            {
                                Register(Split(k));
                            }
                            if (indexSet.TryGetValue(key, out sortedValue))
                            {
                                var rvalues = sortedValue.Get(MKeys[keyIndex], values.ToArray());
                                if (rvalues.Count > 0)
                                {
                                    return Clone(rvalues)[0];
                                }
                            }
                        }
                        return default(T);
                    }
                }
                var val = this.persistent.Get(GetFeilds(this.MKeys[keyIndex]), values.ToArray());
                if (val != null)
                {
                    Register(Split(val));
                    return val;
                }
            }
            return null;
        }
        public string[] MainKey => GetFeilds(MKeys[0]);
        String[] GetFeilds(int[] m)
        {
            var l = new List<String>();
            foreach (var r in m)
            {
                l.Add(this.properties[r].Name);
            }
            return l.ToArray();
        }
        Object HotCache.ISynchronize.Cache(Hashtable value)
        {
            if (value != null)
            {
                T v = Activator.CreateInstance<T>();

                Reflection.SetProperty(v, value);

                return GetCache(v);
            }
            return null;
        }
        void HotCache.ISynchronize.SyncData(Hashtable data, String ip)
        {
            if (data != null)
            {
                var value = data["value"] as Hashtable;
                if (value != null)
                {
                    T v = Activator.CreateInstance<T>();

                    Reflection.SetProperty(v, value);

                    this.Delete(v, false);

                    if (this.IsSyncData || this.IsFast)
                    {
                        var values = new List<Object>();
                        int keyIndex;
                        var key = GetKey(Split(v), out keyIndex, values);
                        if (key != 0 && keyIndex == 0)
                        {
                            var val = this.persistent.Get(GetFeilds(this.MKeys[keyIndex]), values.ToArray());
                            if (val != null)
                            {
                                Register(Split(val));
                            }
                        }
                    }
                }
            }
        }
        int HotCache.ISynchronize.Length
        {
            get
            {
                return dataCache.Count;
            }
        }
        void HotCache.ISynchronize.Clear()
        {
            isSearchAll = false;
            foreach (var v in this.indexKeys)
            {
                v.Clear();
            }
            this.dataCache.Clear();
        }
        public T[] Get(T t, String field, params object[] fieldVals)
        {

            var keyValue = UMC.Data.Reflection.PropertyToDictionary(t);

            var fieldValues = new List<Object>(fieldVals);
            if (String.IsNullOrEmpty(field) == false)
            {
                if (keyValue.Contains(field))
                {
                    var v = keyValue[field];
                    if (fieldValues.Contains(v) == false)
                    {
                        fieldValues.Add(v);
                    }
                }
                else if (fieldVals.Length > 0)
                {
                    keyValue[field] = fieldVals[0];
                }
            }
            var res = new List<T>();
            var values = new List<Object>();
            int keyIndex = GetKeyValueIndex(keyValue);
            if (keyIndex > -1)
            {
                var Ismd5 = this.MKeys[keyIndex].Length > 4;
                foreach (var k in fieldValues)
                {
                    keyValue[field] = k;
                    ulong endValue;
                    var mkeyValue = new List<object>();
                    var mValue = GetHashCode(keyValue, keyIndex, out endValue, mkeyValue);

                    var searchValues = mkeyValue.ToArray();
                    if (mValue != 0)
                    {
                        if (Ismd5 || mValue == endValue)
                        {
                            var m = mValue;
                            if (keyIndex > 0)
                            {
                                CacheValue value;
                                if (indexKeys[keyIndex - 1].TryGetValue(mValue, out value))
                                {
                                    res.AddRange(Clone(value.Get(this.MKeys[keyIndex], searchValues)));
                                }
                                else
                                {
                                    values.Add(k);
                                }
                            }
                            else
                            {
                                CacheValue value;
                                if (dataCache.TryGetValue(m, out value))
                                {
                                    res.AddRange(Clone(value.Get(this.MKeys[keyIndex], searchValues)));
                                }
                                else
                                {
                                    values.Add(k);
                                }
                            }
                        }
                        else if (keyIndex == 0)
                        {

                            dataCache.Search(mValue, endValue, v => res.AddRange(Clone(v.Get(this.MKeys[keyIndex], searchValues))));


                            if (res.Count == 0)
                            {
                                values.Add(k);
                            }
                        }
                        else
                        {
                            var indexCache = this.indexKeys[keyIndex - 1];

                            indexCache.Search(mValue, endValue, v => res.AddRange(Clone(v.Get(MKeys[keyIndex], searchValues))));

                            if (res.Count == 0)
                            {
                                values.Add(k);
                            }
                        }
                    }
                }
            }
            else
            {
                values.AddRange(fieldVals);
            }

            if (values.Count > 0)
            {
                var vals = this.persistent.Get(t, field, values.ToArray());
                if (vals != null)
                {
                    foreach (var val in vals)
                    {
                        Register(Split(val));
                        res.Add(val);
                    }
                }
            }
            else if (keyValue.Count == 0)
            {
                if (isSearchAll)
                {
                    dataCache.Values(v => res.AddRange(Clone(v.Rows.ToList())));
                }
                else
                {
                    isSearchAll = true;

                    var vals = this.persistent.Get(t, field, values.ToArray());
                    if (vals != null)
                    {
                        foreach (var val in vals)
                        {
                            Register(Split(val));
                            res.Add(val);
                        }
                    }

                }
            }
            return res.ToArray();
        }
        bool isSearchAll = false;
        int GetKeyValueIndex(IDictionary keyValue)
        {
            int indexKey = -1;
            if (keyValue.Count > 0)
            {
                for (var i = 0; i < this.MKeys.Count; i++)
                {
                    int ct = 0;
                    foreach (var c in this.MKeys[i])
                    {
                        if (keyValue.Contains(this.properties[c].Name))
                        {
                            ct++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (ct == keyValue.Count)
                    {
                        indexKey = i;
                    }

                }
            }
            return indexKey;
        }
        ulong HashCode(params Object[] objs)
        {

            switch (objs.Length)
            {
                case 1:
                    return GetHashCode(objs[0]);
                case 2:
                    return (GetHashCode(objs[0]) << 32) | (GetHashCode(objs[1]) << 32 >> 32);
                case 3:
                    return ((GetHashCode(objs[0]) << 42) | (GetHashCode(objs[1]) << 43 >> 21)) | (GetHashCode(objs[2]) << 43 >> 42);
                case 4:
                    return ((GetHashCode(objs[0]) << 48) | (GetHashCode(objs[1]) << 48 >> 16) | (GetHashCode(objs[2]) << 48 >> 32)) + (GetHashCode(objs[3]) << 48 >> 48);
                default:
                    var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] md = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(String.Join("", objs.ToArray())));
                    return BitConverter.ToUInt64(md, 8);
            }

        }
        ulong GetHashCode(int length, out ulong end, List<object> ls)
        {
            switch (length)
            {
                case 1:
                    end = GetHashCode(ls[0]);
                    return end;
                case 2:
                    {
                        ulong l = GetHashCode(ls[0]);
                        if (ls.Count > 1)
                        {
                            end = (l << 32) + (GetHashCode(ls[1]) << 32 >> 32);
                            return end;
                        }
                        else
                        {
                            end = ((l + 1) << 32);
                            return (l << 32);
                        }
                    }
                case 3:
                    {
                        var l = GetHashCode(ls[0]);
                        if (ls.Count > 1)
                        {
                            var l1 = GetHashCode(ls[1]);
                            if (ls.Count > 2)
                            {
                                end = (l << 42) | (l1 << 43 >> 21) | (GetHashCode(ls[2]) << 43 >> 43);
                                return end;
                            }
                            end = (l << 42) | ((l1 + 1) << 43 >> 21);
                            return (l << 42) | (l1 << 43 >> 21);
                        }
                        end = ((l + 1) << 42);
                        return (l << 42);
                    }
                case 4:
                    {
                        var l = GetHashCode(ls[0]);

                        if (ls.Count > 1)
                        {
                            var l1 = GetHashCode(ls[1]);
                            if (ls.Count > 2)
                            {
                                var l2 = GetHashCode(ls[2]);
                                if (ls.Count > 3)
                                {
                                    end = ((l << 48) | (l1 << 48 >> 16) | (l2 << 46 >> 32)) + (GetHashCode(ls[3]) << 46 >> 46);
                                    return end;
                                }
                                end = (l << 48) | (l1 << 48 >> 16) | ((l2 + 1) << 46 >> 32);
                                return (l << 48) | (l1 << 48 >> 16) | (l2 << 46 >> 32);
                            }
                            else
                            {
                                end = (l << 48) | ((l1 + 1) << 48 >> 16);
                                return (l << 48) | (l1 << 48 >> 16);
                            }
                        }
                        end = ((l + 1) << 48);
                        return (l << 48);
                    }
                default:
                    if (length == ls.Count)
                    {
                        var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                        byte[] md = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(String.Join("", ls.ToArray())));

                        end = BitConverter.ToUInt64(md, 8);
                        return end;
                    }
                    else
                    {
                        end = 0;
                        return 0;
                    }


            }
        }
        ulong GetHashCode(IDictionary keyValue, int index, out ulong end, List<object> value)
        {
            var ls = value;
            foreach (var c in this.MKeys[index])
            {
                if (keyValue.Contains(this.properties[c].Name))
                {
                    ls.Add(keyValue[this.properties[c].Name]);
                }
                else
                {
                    break;
                }
            }
            return GetHashCode(this.MKeys[index].Length, out end, ls);

        }


        public T[] Get(T field, int start, int limit, out int total)
        {
            return this.persistent.Get(field, start, limit, out total);
        }
        void RemoveIndex(object[] value)
        {

            for (var i = 1; i < MKeys.Count; i++)
            {
                var pk = MKeys[i];
                var values = new List<Object>();
                foreach (var p in pk)
                {

                    var va = value[p];
                    if (va != null)
                    {
                        values.Add(va);

                    }
                    else
                    {
                        values.Clear();
                        break;
                    }


                }
                if (values.Count > 0)
                {
                    this.indexKeys[i - 1].Delete(HashCode(values.ToArray()));
                }
            }
        }
        void RegisterIndex(object[] value, CacheValue mainValue)
        {
            for (var i = 1; i < MKeys.Count; i++)
            {
                var pk = MKeys[i];
                var values = new List<Object>();
                foreach (var p in pk)
                {

                    var va = value[p];
                    if (va != null)
                    {
                        values.Add(va);
                    }
                    else
                    {
                        values.Clear();
                        break;
                    }
                }
                if (values.Count > 0)
                {
                    indexKeys[i - 1].Put(HashCode(values.ToArray()), mainValue);
                }
            }
        }
        void Register(object[] value)
        {

            CacheValue mainKey = null;

            for (var i = 0; i < MKeys.Count; i++)
            {
                var pk = MKeys[i];
                var values = new List<Object>();
                foreach (var p in pk)
                {

                    var va = value[p];
                    if (va != null)
                    {
                        values.Add(va);
                    }
                    else
                    {
                        values.Clear();
                        break;
                    }


                }
                if (values.Count > 0)
                {
                    var key = HashCode(values.ToArray());
                    if (i == 0)
                    {
                        if (dataCache.TryGetValue(key, out mainKey))
                        {
                            var mValue = mainKey.Get(this.MKeys[0], values.ToArray());
                            if (mValue.Count > 0)
                            {
                                RemoveIndex(mValue[0]);
                            }
                            mainKey.Put(this.MKeys[0], value);

                        }
                        else
                        {
                            mainKey = new CacheValue();

                            mainKey.Put(this.MKeys[0], value);
                            dataCache.Put(key, mainKey);
                        }
                    }
                    else
                    {
                        indexKeys[i - 1].Put(key, mainKey);

                    }

                }
                else if (mainKey == null)
                {
                    return;

                }
            }
        }
        public void Delete(T value)
        {
            Delete(value, true);
        }
        void Delete(T value, bool isback)
        {
            var values = new List<Object>();
            int keyIndex;
            var key = GetKey(Split(value), out keyIndex, values);
            if (key != 0 && keyIndex == 0)
            {
                CacheValue sortedValue;
                if (dataCache.TryGetValue(key, out sortedValue))
                {
                    var vs = sortedValue.Remove(this.MKeys[0], values.ToArray());
                    if (vs != null)
                    {
                        RemoveIndex(vs);
                    }
                    if (sortedValue.Count == 0)
                    {
                        dataCache.Delete(key);
                    }
                }

                if (isback)
                {
                    this.Delete(GetFeilds(this.MKeys[keyIndex]), values.ToArray());
                }
            }
            else if (isback)
            {
                throw new ArgumentException("只支持主键删除");
            }



        }
        ulong GetHashCode(object obj)
        {
            if (obj is DateTime)
            {
                return Convert.ToUInt32(UMC.Data.Utility.TimeSpan((DateTime)obj));
            }
            else if (obj is String)
            {
                return BitConverter.ToUInt64(Utility.MD5(obj), 0);
            }
            else if (obj is Int64)
            {
                return BitConverter.ToUInt64(BitConverter.GetBytes((Int64)obj), 0);
            }
            else if (obj is UInt64)
            {
                return (UInt64)obj;
            }
            else if (obj is Guid)
            {
                var g = (Guid)obj;
                var gb = g.ToByteArray();
                var b = new byte[8];
                for (var i = 0; i < 8; i++)
                {
                    b[i] = gb[i * 2 + 1];
                }
                return BitConverter.ToUInt64(b, 0);
            }

            return BitConverter.ToUInt32(BitConverter.GetBytes(obj.GetHashCode()), 0);
        }

        ulong GetKey(object[] t, out int keyIndex, List<Object> values)
        {
            keyIndex = -1;
            for (var i = 0; i < MKeys.Count; i++)
            {
                keyIndex = i;
                var pk = MKeys[i];
                foreach (var p in pk)
                {
                    var va = t[p];
                    if (va != null)
                    {
                        values.Add(va);
                    }
                    else
                    {
                        values.Clear();
                        break;
                    }
                }
                if (values.Count > 0)
                {
                    break;
                }
            }
            if (values.Count == 0)
            {
                return 0;
            }
            else
            {
                return HashCode(values.ToArray());
            }
        }

    }
}
