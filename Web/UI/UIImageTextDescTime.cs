using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIImageTextDescTime : UICell
    {
        public UIImageTextDescTime(Uri src, string text, string desc, string time)
        {
            this.data = new WebMeta().Put("text", text, "desc", desc, "time", time).Put("src", src.AbsoluteUri);
            this.Type = "ImageTextDescTime";
        }
        public UIImageTextDescTime Click(UIClick click)
        {

            this.data.Put("click", click);
            return this;
        }
        public UIImageTextDescTime(WebMeta desc)
        {
            this.data = desc;
            this.Type = "ImageTextDescTime";

        }
        public UIImageTextDescTime Text(String text)
        {
            this.Format.Put("text", text);
            return this;
        }
        public UIImageTextDescTime Value(String value)
        {
            this.Format.Put("value", value);
            return this;
        }
        public UIImageTextDescTime Tag(String tag)
        {
            this.Format.Put("tag", tag);
            return this;
        }
        WebMeta data;
        public override object Data => data;
    }
}
