using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIProduct : UICell
    {

        List<WebMeta> items = new List<WebMeta>();
        public UIProduct()
        {
            this.data = new WebMeta().Put("data", this.items); ;
            this.Type = "Products";
        }
        public UIProduct Add(String src, string id, string name, decimal price, decimal origin)
        {
            this.items.Add(new WebMeta().Put("src", src, "name", name).Put("id", id).Put("price", price).Put("origin", origin));
            return this;
        }
        public UIProduct Add(String src, UIClick click, string name, decimal price, decimal origin)
        {
            this.items.Add(new WebMeta().Put("src", src, "name", name).Put("click", click).Put("price", price).Put("origin", origin));
            return this;
        }
        public UIProduct Add(String src, UIClick click, string name, decimal price)
        {
            this.items.Add(new WebMeta().Put("src", src, "name", name).Put("click", click).Put("price", price));
            return this;
        }
        public UIProduct Add(String src, string id, string name, decimal price)
        {
            this.items.Add(new WebMeta().Put("src", src, "name", name).Put("id", id).Put("price", price));
            return this;
        }
        public UIProduct Add(WebMeta item)
        {
            this.items.Add(item);// new WebMeta().Put("title", title, "desc", desc).Put("src", src));
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
