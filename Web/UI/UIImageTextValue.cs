using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UMC.Web.UI
{
    public class UIImageTextValue : UICell
    {
        private UIImageTextValue(WebMeta data)
        {
            this.data = data;
        }

        public UIImageTextValue(String src, String text, String value)
        {
            this.data = new WebMeta().Put("src", src);
            this.data.Put("text", text).Put("value", value);
            this.Type = "ImageTextValue";
            // return t;

        }
        public UIImageTextValue Text(string text)
        {
            this.Format.Put("text", text);
            return this;
        }
        public UIImageTextValue Value(string value)
        {
            this.Format.Put("value", value);
            return this;
        }
        //public UIImageTextValue Put(String name, String value)
        //{
        //    data.Put(name, value);
        //    return this;

        //}
        public UIImageTextValue Click(UIClick click)
        {
            data.Put("click", click);
            return this;

        }
        private WebMeta data;
        public override object Data => data;

    }
}
