using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIProductPrice : UICell
    {
        public UIProductPrice()
        {
            this.data = new WebMeta();
            this.Type = "ProBuy";
        }
        public UIProductPrice(WebMeta meta)
        {
            this.data = meta;
            this.Type = "ProBuy";
        }
        WebMeta data;
        public override object Data => data;

        public UIProductPrice Title(String title)
        {
            this.Format.Put("title", title);

            return this;
        }
        public UIProductPrice Value(String value)
        {
            this.Format.Put("value", value);

            return this;
        }
        public UIProductPrice Price(String price)
        {
            this.Format.Put("price", price);

            return this;
        }
        public UIProductPrice Caption(String caption)
        {
            this.Format.Put("caption", caption);

            return this;
        }
        public UIProductPrice Tag(String tag)
        {
            this.Format.Put("tag", tag);

            return this;
        }
    }
}
