using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIDesc : UICell
    {
        public UIDesc(string desc)
        {
            this.data = new WebMeta().Put("desc", desc);
            this.Type = "Desc";
        }
        public UIDesc Click(UIClick click)
        {

            this.data.Put("click", click);
            return this;
        }
        public UIDesc(WebMeta desc)
        {
            this.data = desc;// new POSMeta().Put("desc", desc);
            this.Type = "Desc";

        }
        public UIDesc Desc(String desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
        WebMeta data;
        public override object Data => data;
    }
}
