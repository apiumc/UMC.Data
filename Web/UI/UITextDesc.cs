using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UITextDesc : UICell
    {
        public UITextDesc(String title, string desc, string tag)
        {
            this.data = new WebMeta().Put("desc", desc, "title", title, "tag", tag);
            this.Type = "TextDesc";
        }
        public UITextDesc Click(UIClick click)
        {

            this.data.Put("click", click);
            return this;
        }
        public UITextDesc(WebMeta desc)
        {
            this.data = desc;// new POSMeta().Put("desc", desc);
            this.Type = "TextDesc";

        }
        public UITextDesc Desc(String desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
        public UITextDesc Tag(String tag)
        {
            this.Format.Put("tag", tag);
            return this;
        }
        public UITextDesc Title(String title)
        {
            this.Format.Put("title", title);
            return this;
        }
        WebMeta data;
        public override object Data => data;
    }
}
