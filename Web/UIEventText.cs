using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web
{

    public class UIEventText : UMC.Data.IJSON
    {
        WebMeta meta = new WebMeta();
        public UIEventText Key(String key)
        {
            meta.Put("key", key);
            return this;
        }
        public UIEventText() { }
        UIStyle _style = new UIStyle();
        public UIEventText Style(UIStyle style)
        {
            _style.Copy(style);

            return this;

        }
        public UIEventText Flex(float flex)
        {

            meta.Put("flex", flex);
            return this;
        }
        public UIEventText Badge(string badge)
        {
            meta.Put("badge", badge);
            return this;
        }
        public UIEventText(String text)
        {
            meta.Put("text", text);
            meta.Put("format", "{text}");
        }
        public UIEventText Src(string src)
        {
            meta.Put("src", src);
            return this;

        }
        public UIEventText Icon(String icon, string color)
        {
            meta.Put("icon", icon);
            _style.Name("icon").Name("color", color);
            return this;
        }
        public UIEventText Icon(char icon, int color)
        {

            meta.Put("icon", icon);

            _style.Name("icon").Color(color);
            return this;

        }
        public UIEventText Icon(char icon)
        {
            meta.Put("icon", icon);
            meta.Put("format", "{icon}");
            this.Style(new UIStyle().Name("icon", new UIStyle().Font("wdk").Size(20)));
            return this;
        }
        public UIEventText(char icon, String text)
        {
            meta.Put("icon", icon);
            meta.Put("text", text);
            meta.Put("format", "{icon}\n{text}");
            this.Style(new UIStyle().Size(8).Name("icon", new UIStyle().Font("wdk").Size(20)).Color(0x666));
        }
        public UIEventText Init(UIClick init)
        {
            meta.Put("init", init);
            return this;
        }
        public UIEventText Format(String format)
        {
            meta.Put("format", format);
            return this;

        }
        void IJSON.Read(string key, object value)
        {

        }
        void IJSON.Write(System.IO.TextWriter writer)
        {
            if (_style.Count > 0)
            {
                this.meta.Put("style", _style);
            }
            UMC.Data.JSON.Serialize(this.meta, writer);

        }


        public UIEventText Click(UIClick click)
        {
            meta.Put("click", click);
            return this;
        }
        public UIEventText Click(String click)
        {
            meta.Put("click", click);
            return this;
        }
    }
}
