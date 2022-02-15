using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace UMC.Web
{

    /// <summary>
    /// Select列表选项
    /// </summary> 
    public sealed class ListItem : UMC.Data.IJSON
    {
        public ListItem()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public ListItem(string text) : this(text, text) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public ListItem(string text, string value) : this(text, value, false) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <param name="select"></param>
        public ListItem(string text, string value, bool selected)
        {
            this.Selected = selected;
            this.Text = text;
            this.Value = value;
        }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Title { get; set; }
        void Data.IJSON.Write(System.IO.TextWriter writer)
        {
            writer.Write("{");

            writer.Write("\"Text\":");
            UMC.Data.JSON.Serialize(this.Text, writer);
            writer.Write(",\"Value\":");
            UMC.Data.JSON.Serialize(this.Value, writer);
            if (String.IsNullOrEmpty(Title) == false)
            {
                writer.Write(",\"Title\":");
                UMC.Data.JSON.Serialize(this.Title, writer);
            }
            if (this.Disabled)
            {
                writer.Write(",\"Disabled\":");
                UMC.Data.JSON.Serialize(this.Disabled, writer);
            }
            if (this.Selected)
            {
                writer.Write(",\"Selected\":");
                UMC.Data.JSON.Serialize(this.Selected, writer);
            }
            writer.Write("}");

        }

        void Data.IJSON.Read(string key, object value)
        {
            switch (key)
            {
                case "Title":
                    this.Title = value as string;
                    break;
                case "Text":
                    this.Text = value as string;
                    break;
                case "Value":
                    this.Value = value as string;
                    break;

                case "Disabled":
                    this.Disabled = String.Equals("true", value as string);
                    break;
                case "Selected":
                    this.Selected = String.Equals("true", value as string);
                    break;


            }
        }
    }
    /// <summary>
    /// Select选项列表
    /// </summary>
    public sealed class ListItemCollection : List<ListItem>
    {
        public void Add(string text)
        {
            this.Add(new ListItem(text));
        }
        public void Add(string text, string value)
        {
            this.Add(new ListItem(text, value));
        }
        public void Add(string text, string value, bool selected)
        {
            this.Add(new ListItem(text, value, selected));
        }
        public ListItemCollection Put(string text, string value)
        {
            this.Add(text, value);
            return this;
        }
        public ListItemCollection Put(string text)
        {
            this.Add(text);
            return this;
        }
        public ListItemCollection Put(string text, string value, bool selected)
        {
            this.Add(text, value, selected);
            return this;
        }

        public ListItemCollection Put(string title, string text, string value)
        {
            this.Add(new ListItem(text, value) { Title = title });
            return this;
        }
    }
}
