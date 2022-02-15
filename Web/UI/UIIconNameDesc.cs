using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UMC.Web.UI
{
    public class UIIconNameDesc : UICell
    {
        public class Item : UMC.Data.IJSON
        {
            WebMeta meta = new WebMeta();

            public void Read(string key, object value)
            {

            }
            public Item Click(UIClick click)
            {

                meta.Put("click", click);
                return this;
            }

            public void Write(TextWriter writer)
            {
                UMC.Data.JSON.Serialize(this.meta, writer);
            }
            public Item(char icon, string name, string desc)
            {
                meta.Put("icon", icon);
                meta.Put("name", name);
                meta.Put("desc", desc);
            }
            public Item(string src, string name, string desc)
            {
                meta.Put("src", src);
                meta.Put("name", name);
                meta.Put("desc", desc);
            }
            public Item(string name, string desc)
            {
                meta.Put("name", name);
                meta.Put("desc", desc);
            }
            public Item Color(int color)
            {
                if (color < 0x1000)
                {
                    meta.Put("color", String.Format("#{0:x3}", color));
                }
                else
                {
                    meta.Put("color", String.Format("#{0:x6}", color));
                }
                return this;
            }
        }
        public UIIconNameDesc Put(String name, object v)
        {
            data.Put(name, v);
            return this;
        }
        public UIIconNameDesc(params Item[] ns)
        {
            this.data = new WebMeta();
            if (ns.Length > 0)
                this.data.Put("items", ns);
            this.Type = "IconNameDesc";
        }
        public UIIconNameDesc Button(string name, UIClick click, int color)
        {

            this.data.Put("button-click", click);
            this.data.Put("button", name);
            this.Style.Name("button").BgColor(color);
            if (color < 0x1000)
            {
                data.Put("button-color", String.Format("#{0:x3}", color));
            }
            else
            {
                data.Put("button-color", String.Format("#{0:x6}", color));
            }
            return this;
        }

        WebMeta data;
        public override object Data => data;
    }
}
