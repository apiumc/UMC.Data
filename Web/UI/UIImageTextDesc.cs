using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIImageTextDesc : UICell
    {
        public UIImageTextDesc(string src, String title, string desc, string tag)
        {
            this.data = new WebMeta().Put("desc", desc, "title", title, "tag", tag, "src", src);
            this.Type = "ImageTextDesc";
        }
        public UIImageTextDesc Click(UIClick click)
        {

            this.data.Put("click", click);
            return this;
        }
        public UIImageTextDesc(WebMeta desc)
        {
            this.data = desc;
            this.Type = "ImageTextDesc";

        }
        public UIImageTextDesc Desc(String desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
        public UIImageTextDesc Tag(String tag)
        {
            this.Format.Put("tag", tag);
            return this;
        }
        public UIImageTextDesc Title(String title)
        {
            this.Format.Put("title", title);
            return this;
        }
        WebMeta data;
        public override object Data => data;
    }
}
