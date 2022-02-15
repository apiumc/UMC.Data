using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UMC.Data;

namespace UMC.Web
{


    public class UIClick : UMC.Data.IJSON
    {
        public String Text
        {
            get; set;
        }
        public static UIClick Product(String id)
        {
            return new UIClick(id) { Key = "Product" };
        }
        public static UIClick Search()
        {
            return new UIClick() { Key = "Search" };
        }
        public static UIClick Search(string type)
        {
            return new UIClick(type) { Key = "Search" };
        }
        public static UIClick Search(string model, String cmd)
        {
            return new UIClick(new WebMeta().Put("model", model).Put("cmd", cmd)) { Key = "Search" };
        }
        public static UIClick Pager(String model, String cmd, WebMeta search)
        {
            var key = new WebMeta().Put("model", model, "cmd", cmd).Put("search", search);
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Pager(String model, String cmd, WebMeta search, bool isCache)
        {

            var key = new WebMeta().Put("model", model, "cmd", cmd).Put("search", search);
            if (isCache)
            {
                key.Put("Cache", isCache);
            }
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Pager(String model, String cmd)
        {
            var key = new WebMeta().Put("model", model, "cmd", cmd);
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Pager(String model, String cmd, bool isCache, String closeEvent)
        {
            var key = new WebMeta().Put("model", model, "cmd", cmd);
            key.Put("CloseEvent", closeEvent);
            if (isCache)
            {
                key.Put("Cache", isCache);
            }
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Pager(String model, String cmd, WebMeta search, bool isCache, String closeEvent)
        {

            var key = new WebMeta().Put("model", model, "cmd", cmd).Put("search", search);
            key.Put("CloseEvent", closeEvent);
            if (isCache)
            {
                key.Put("Cache", isCache);
            }
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Pager(String model, String cmd, bool isCache)
        {
            var key = new WebMeta().Put("model", model, "cmd", cmd);
            if (isCache)
            {
                key.Put("Cache", isCache);
            }
            return new UIClick(key) { Key = "Pager" };
        }
        //public static UIClick BarCode(UIClick click)
        //{
        //    var cl = new UIClick() { Key = "BarCode" };
        //    cl._send = click;
        //    return cl;
        //}
        public static UIClick Pager(String model, String cmd, String refreshEvent)
        {
            var key = new WebMeta().Put("model", model, "cmd", cmd);
            key.Put("RefreshEvent", refreshEvent);
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Pager(String model, String cmd, WebMeta search, String refreshEvent)
        {
            var key = new WebMeta().Put("model", model, "cmd", cmd).Put("search", search);
            key.Put("RefreshEvent", refreshEvent);
            return new UIClick(key) { Key = "Pager" };
        }
        public static UIClick Url(Uri url)
        {
            return new UIClick(url.AbsoluteUri) { Key = "Url" };
        }
        public static UIClick Tel(String tel)
        {
            return new UIClick(tel) { Key = "Tel" };
        }
        public static UIClick Scanning()
        {
            return new UIClick() { Key = "Scanning" };
        }
        public static UIClick Scanning(UIClick click)
        {
            var t = new UIClick() { Key = "Scanning" };
            t._send = click;
            return t;
        }

        public static UIClick Map(String location, String address, params WebMeta[] items)
        {
            return new UIClick(new WebMeta().Put("location", location, "address", address).Put("items", items)) { Key = "Map" };
        }
        public static UIClick Map(String address, params WebMeta[] items)
        {
            if (items.Length > 0)
            {

                return new UIClick(new WebMeta().Put("address", address).Put("items", items)) { Key = "Map" };
            }
            else
            {
                return new UIClick(address) { Key = "Map" };
            }
        }
        public static UIClick Category(String id)
        {

            return new UIClick(new WebMeta().Put("Id", id)) { Key = "Category" };
        }
        public static UIClick Category(String id, String title)
        {
            return new UIClick(new WebMeta().Put("Id", id, "Title", title)) { Key = "Category" };
        }
        public static UIClick IM(WebMeta im)
        {
            return new UIClick(im) { Key = "IM" };
        }
        public static UIClick Click(UIClick click)
        {
            var c = new UIClick() { Key = "Click" };
            c._send = click;
            return c;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static UIClick IM()
        {
            return new UIClick() { Key = "IM" };
        }
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static UIClick Subject(String id)
        {
            return new UIClick(id) { Key = "Subject" };
        }
        /// <summary>
        /// 界面查询
        /// </summary>
        /// <param name="query">查询字典对</param>
        /// <returns></returns>
        public static UIClick Query(WebMeta query)
        {
            return new UIClick(query) { Key = "Query" };
        }
        /// <summary>
        /// 界面查询
        /// </summary>
        /// <param name="query">查询字典对</param>
        /// <param name="cache">是否缓存</param>
        /// <returns></returns>
        public static UIClick Query(WebMeta query, bool cache)
        {
            return cache ? new UIClick(new WebMeta(query).Put("cache", true)) { Key = "Query" } : new UIClick(query) { Key = "Query" };
        }
        /// <summary>
        /// 界面查询
        /// </summary>
        /// <param name="key">Section界面Key</param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static UIClick Query(String key, WebMeta query)
        {
            return new UIClick(new WebMeta().Put("key", key).Put("send", query)) { Key = "Query" };
        }
        public UIClick() { }
        public UIClick(String send)
        {
            this._send = send;
        }
        public UIClick(params String[] keys)
        {
            this._send = new WebMeta().Put(keys); ;
        }
        public UIClick(WebMeta send)
        {
            this._send = send;
        }
        public UIClick Send(WebMeta send)
        {
            this._send = send;
            return this;

        }

        public UIClick Send(String send)
        {
            this._send = send;
            return this;

        }
        public String Key
        {
            get; set;
        }
        private object _send;
        public String Model
        {
            get; set;
        }
        public String Command
        {
            get; set;
        }
        public String Value
        {
            get; set;
        }

        public Object Send()
        {
            return _send;

        }

        void IJSON.Read(string key, object value)
        {
            switch (key)
            {
                case "key":
                    this.Key = value as string;
                    break;
                case "send":
                    this._send = value;
                    break;
                case "model":
                    this.Model = value as string;
                    break;
                case "cmd":
                    this.Command = value as string;
                    break;
            }
        }
        void IJSON.Write(System.IO.TextWriter writer)
        {
            writer.Write("{");
            if (String.IsNullOrEmpty(this.Key) == false)
            {
                UMC.Data.JSON.Serialize("key", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.Key, writer);
                if (this._send != null)
                {

                    writer.Write(",");
                    UMC.Data.JSON.Serialize("send", writer); writer.Write(":");
                    UMC.Data.JSON.Serialize(this._send, writer);


                }
            }
            else
            {

                UMC.Data.JSON.Serialize("model", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(this.Model, writer);
                writer.Write(",");
                UMC.Data.JSON.Serialize("cmd", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(this.Command, writer);

                if (this._send != null)
                {

                    writer.Write(",");
                    UMC.Data.JSON.Serialize("send", writer); writer.Write(":");
                    UMC.Data.JSON.Serialize(this._send, writer);


                }
            }
            if (String.IsNullOrEmpty(Value) == false)
            {
                writer.Write(",");

                UMC.Data.JSON.Serialize("value", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.Value, writer);
            }

            if (String.IsNullOrEmpty(Text) == false)
            {
                writer.Write(",");
                UMC.Data.JSON.Serialize("text", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.Text, writer);


            }
            writer.Write("}");
        }

        public UIClick Send(string model, string cmd)
        {
            this.Model = model; this.Command = cmd;
            return this;
        }
    }
}