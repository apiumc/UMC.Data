using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIItems : UICell
    {

        List<WebMeta> items = new List<WebMeta>();
        public UIItems()
        {
            this.data = new WebMeta().Put("items", this.items); ;
            this.Type = "UIItems";
        }
        public UIItems Add(String src, string title, string desc)
        {
            this.items.Add(new WebMeta().Put("title", title, "desc", desc).Put("src", src));
            return this;
        }
        public UIItems Add(WebMeta item)
        {
            this.items.Add(item);// new WebMeta().Put("title", title, "desc", desc).Put("src", src));
            return this;
        }
        public UIItems Add(String src, string title, string desc, int startColor, int endColor)
        {
            this.items.Add(new WebMeta().Put("title", title, "desc", desc).Put("src", src).Put("startColor", UIStyle.ToColor(startColor)).Put("endColor", UIStyle.ToColor(endColor)));
            return this;
        }
        public UIItems Add(UIClick click, String src, string title, string desc)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("title", title, "desc", desc).Put("src", src));
            return this;
        }
        public UIItems Add(UIClick click, String src, string title, string desc, int startColor, int endColor)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("src", src).Put("title", title, "desc", desc).Put("startColor", UIStyle.ToColor(startColor)).Put("endColor", UIStyle.ToColor(endColor)));
            return this;
        }
        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }
        private WebMeta data;
        public override object Data => data;

    }
}
