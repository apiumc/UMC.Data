using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web
{

    public class UITitle : UMC.Data.IJSON
    {
        public UITitle(String title)
        {
            meta.Put("text", title);
        }
        public UITitle Name(String name, string value)
        {
            meta.Put(name, value);
            return this;
        }
        public UITitle Items(params string[] value)
        {
            if (value.Length > 0)
            {
                var items = new List<WebMeta>();
                meta.Put("type", "Banner");
                meta.Put("items", items);
                Utility.Each(value, v => items.Add(new WebMeta().Put("text", v)));
            }
            return this;
        }
        public UITitle Items(params UIClick[] value)
        {
            if (value.Length > 0)
            {

                var items = new List<WebMeta>();
                meta.Put("type", "Banner");
                meta.Put("items", items);
                Utility.Each(value, v => items.Add(new WebMeta().Put("text", v.Text).Put("style", new UIStyle().Click(v))));
            }
            return this;
        }
        private UITitle()
        {

        }
        public static UITitle Create()
        {
            return new UITitle();
        }
        public String Title
        {
            get
            {
                return meta.Get("text");
            }
            set
            {

                meta.Put("text", value);
            }
        }
        public UITitle Float()
        {
            meta.Put("float", true);
            return this;
        }
        public UITitle Left(char icon, UIClick click)
        {
            this.Left(new UIEventText().Icon(icon).Click(click));
            return this;

        }
        public UITitle Left(UIEventText text)
        {
            meta.Put("left", text);
            return this;
        }
        public UIStyle Style
        {
            get { return _Style; }
        }
        UIStyle _Style = new UIStyle();

        public UITitle Left(char icon, String click)
        {
            this.Left(new UIEventText().Icon(icon).Click(click));
            return this;
        }
        public UITitle Right(char icon, String click)
        {
            this.Right(new UIEventText().Icon(icon).Click(click));
            return this;
        }
        public UITitle Right(UIEventText text)
        {
            meta.Put("right", text);
            return this;
        }

        public UITitle Right(char icon, UIClick click)
        {
            this.Right(new UIEventText().Icon(icon).Click(click));
            return this;
        }
        public UITitle(Uri video, String src)
        {

            this.meta.Put("type", "Video");
            this.meta.Put("video-src", video.AbsoluteUri);
            this.meta.Put("src", src);
        }
        public static UITitle TabTitle()
        {
            var t = new UITitle();
            t.meta.Put("type", "Tab");
            return t;
        }
        WebMeta meta = new WebMeta();
        void IJSON.Read(string key, object value)
        {

        }
        void IJSON.Write(System.IO.TextWriter writer)
        {
            if (_Style.Count > 0)
            {
                this.meta.Put("style", _Style);
            }
            UMC.Data.JSON.Serialize(this.meta, writer);

        }

    }
}
