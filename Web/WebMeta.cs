using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{
    /// <summary>
    /// 头元信息
    /// </summary># 
    public class WebMeta : UMC.Data.IJSON
    {
        internal void Set(IDictionary dic)
        {
            this._Dictionary.Clear();
            var em = dic.GetEnumerator();
            while (em.MoveNext())
            {
                this._Dictionary[em.Key] = em.Value;
            }

        }
        /// <summary>
        /// 获取字典
        /// </summary>
        /// <returns></returns>
        public System.Collections.Hashtable GetDictionary()
        {
            return this._Dictionary;

        }
        /// <summary>
        /// 初始化
        /// </summary>
        public WebMeta()
        {
            this._Dictionary = new Hashtable();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="metas"></param>
        public WebMeta(string Key, params WebMeta[] metas)
            : this()
        {
            this.Set(Key, metas);
        }
        /// <summary>
        /// 复制字典并转化为WebMeta
        /// </summary>
        /// <param name="task"></param>
        public WebMeta(IDictionary task)
        {
            this._Dictionary = new Hashtable(task);
        }
        /// <summary>
        /// 复制meta
        /// </summary>
        /// <param name="meta"></param>
        public WebMeta(WebMeta meta)
        {
            this._Dictionary = new Hashtable(meta.GetDictionary());
        }

        public WebMeta(params string[] keys) : this()
        {
            if (keys.Length == 1)
            {
                this["type"] = keys[0];
            }
            else
            {
                for (int i = 0; i < keys.Length; i = i + 2)
                {
                    if (i + 1 < keys.Length)
                    {
                        this[keys[i]] = keys[i + 1];
                    }
                }
            }
        }
        /// <summary>
        /// 获取设置头信息字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                if (value == null)
                {
                    _Dictionary.Remove(key);
                }
                else
                {
                    _Dictionary[key] = value;
                }
            }
        }
        Hashtable _Dictionary;
        /// <summary>
        /// 获取信息字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            var obj = _Dictionary[key];
            return obj as string;
        }
        /// <summary>
        /// 获取元信息
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public WebMeta GetMeta(string key)
        {
            var obj = _Dictionary[key];
            if (obj is WebMeta)
            {
                return (WebMeta)obj;
            }
            if (obj is IDictionary)
            {
                return new WebMeta(obj as IDictionary);
            }
            return null;
        }
        /// <summary>
        /// Determines whether the System.Collections.Hashtable contains a specific key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return this._Dictionary.Contains(key);
        }
        /// <summary>
        /// 移除信息
        /// </summary>
        /// <param name="key">Key</param>
        public void Remove(string key)
        {
            _Dictionary.Remove(key);
        }
        public WebMeta Put(string key, string value)
        {

            this[key] = value;


            return this;
        }
        public WebMeta Put(params string[] keys)
        {
            for (int i = 0; i < keys.Length; i = i + 2)
            {
                if (i + 1 < keys.Length)
                {
                    this[keys[i]] = keys[i + 1];
                }
            }
            return this;
        }
        /// <summary>
        /// 设置子key头信息
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="meta">头元信息</param>
        public void Set(string key, WebMeta meta)
        {
            _Dictionary[key] = meta.GetDictionary();
        }
        public WebMeta Put(string key, object value)
        {
            if (value == null)
            {
                _Dictionary.Remove(key);
            }
            else
            {

                _Dictionary[key] = value;
            }
            return this;
        }
        /// <summary>
        /// 设置值类型值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="task"></param>
        internal void Set(string key, IDictionary task)
        {
            _Dictionary[key] = task;
        }
        /// <summary>
        /// 设置值类型值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="metas"></param>
        internal void Set(string key, params WebMeta[] metas)
        {
            if (metas.Length > 0)
            {
                switch (metas.Length)
                {
                    case 0:
                        break;
                    case 1:
                        _Dictionary[key] = metas[0];
                        break;
                    default:
                        _Dictionary[key] = metas;
                        break;
                }
            }
        }
        /// <summary>
        /// 清空元头信息
        /// </summary>
        public void Clear()
        {
            this._Dictionary.Clear();
        }
        /// <summary>
        /// 集合数量
        /// </summary>
        public int Count
        {
            get
            {
                return this._Dictionary.Count;
            }
        }

        public WebMeta UIEvent(string name, object value)
        {
            this.Put("type", "UI.Event", "key", name).Put("value", value);
            return this;
        }
        public WebMeta Cell(Web.UICell cell)
        {
            this.Put("_CellName", cell.Type).Put("value", cell.Data).Put("format", cell.Format).Put("style", cell.Style);
            return this;
        }
        public WebMeta UIEvent(string name, string ui, object value)
        {
            switch ((ui ?? "none").ToLower())
            {
                case "":
                case "none":
                    this.Put("type", "UI.Event", "key", name).Put("value", value);
                    break;
                default:
                    this.Put("type", "UI.Event", "key", name).Put("ui", ui).Put("value", value);
                    break;
            }
            return this;
        }
        public WebMeta Command(string model, string cmd)
        {

            this.Put("Model", model, "Command", cmd);
            return this;
        }
        public WebMeta Command(string model, string cmd, string value)
        {
            this.Put("SendValue", value, "Model", model, "Command", cmd);
            return this;
        }
        public WebMeta Command(string model, string cmd, WebMeta value)
        {
            this.Put("Model", model, "Command", cmd).Put("SendValue", value);
            return this;
        }
        public WebMeta CloseEvent(params string[] values)
        {
            this.Put("CloseEvent", String.Join(",", values));
            return this;
        }
        public WebMeta RefreshEvent(params string[] values)
        {
            this.Put("RefreshEvent", String.Join(",", values));
            return this;
        }
        public WebMeta NotRequired()
        {
            return this.Put("required", "no");
        }
        public WebMeta PlaceHolder(string placeholder)
        {
            this.Put("placeholder", placeholder);
            return this;
        }

        #region IJSONSerializable Members

        void Data.IJSON.Write(System.IO.TextWriter writer)
        {
            UMC.Data.JSON.Serialize(this._Dictionary, writer);
        }

        void Data.IJSON.Read(string key, object value)
        {
            this._Dictionary[key] = value;
        }

        #endregion
    }
}
