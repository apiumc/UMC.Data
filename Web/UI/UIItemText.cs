using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIItemText : UICell
    {

        public UIItemText(WebMeta data)
        {


            this.data = data;
            this.Type = "ItemText";
        }
        public UIItemText(string text, string desc)
        {
            this.data = new WebMeta().Put("text", text, "desc", desc);
            this.Type = "ItemText";
        }
        WebMeta data;
        public override object Data => data;

        public UIItemText Text(String text)
        {
            this.Format.Put("text", text);
            return this;
        }
        public UIItemText Desc(String desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
        public UIItemText Click(UIClick click)
        {
            data.Put("click", click);
            return this;

        }
    }
}
