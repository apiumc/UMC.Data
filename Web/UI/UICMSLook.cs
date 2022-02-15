using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UICMSLook : UICell
    {
        public override object Data => data;
        public  UICMSLook (String src, UIClick click, WebMeta data)
        {
            this.data = data;
            this.data.Put("src", src);
            this.data.Put("click", click);
            this.Type = "CMSLook"; 

        }
        public UICMSLook(String src, string title, string desc)
        {
            data = new WebMeta();
            data.Put("src", src, "title", title, "desc", desc);
            this.Type = "CMSLook";

        }
        WebMeta data;
        private UICMSLook(WebMeta data)
        {
            this.data = data;
        }
        public UICMSLook Click(UIClick click)
        {

            this.data.Put("click", click);
            return this;
        }
        public UICMSLook Title(String title)
        {
            this.Format.Put("title", title);
            return this;
        }

        public UICMSLook Desc(String desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
    }
}
