using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UISheet : UICell
    {
        List<WebMeta> items = new List<WebMeta>();

        WebMeta data;
        public override object Data => data; public UISheet(String title)
        {
            this.data = new WebMeta().Put("title", title);
            this.data.Put("items", items);
            this.Type = "UISheet";
            this.Style.Name("icon").Font("wdk").Color(0x999);
            this.Style.Name("info").Font("wdk").Color(0xef4f4f);
        }
        public UISheet AddItem(String text, string desc, bool isInfo)
        {
            var b = new WebMeta().Put("text", text, "desc", desc);
            if (isInfo)
            {
                b.Put("icon", '\uF05A');
            }
            else
            {

                b.Put("info", '\uF05D');
            }
            this.items.Add(b);
            return this;
        }
        public UISheet AddItem(String text, string desc)
        {
            this.AddItem(text, desc, false);
            return this;
        }
        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }
    }
}
