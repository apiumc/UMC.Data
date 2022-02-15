using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    
    public class UITabFixed : UICell
    {
        List<Object> items = new List<Object>();
        public UITabFixed()
        {
            this.data = new WebMeta().Put("items", this.items); ;
            this.Type = "TabFixed";
        }
        WebMeta data;
        public override object Data => data;

        public int SelectIndex
        {
            get
            {
                return Utility.IntParse(this.data["selectIndex"], 0);
            }
            set
            {
                this.data.Put("selectIndex", value.ToString());
            }
        }
        public WebMeta SelectValue
        {
            get
            {
                var s = this.SelectIndex;
                if (s < this.items.Count)
                {
                    Object v = this.items[s];
                    if (v is WebMeta)
                    {
                        return (WebMeta)v;
                    }
                }
                return null;
            }
        }

        public UITabFixed Add(String text, String search)
        {
            items.Add(new WebMeta().Put("text", text).Put("search", search));//.Put("Key", key));
            return this;

        }
        public UITabFixed Add(String text, String search, String key)
        {
            items.Add(new WebMeta().Put("text", text).Put("search", search).Put("Key", key));
            return this;
        }
        public UITabFixed Add(String text, WebMeta search)
        {
            items.Add(new WebMeta().Put("text", text).Put("search", search));
            return this;
        }
        public UITabFixed Add(String text, WebMeta search, String key)
        {
            items.Add(new WebMeta().Put("text", text).Put("search", search).Put("Key", key));
            return this;
        }
        public UITabFixed Add(UIClick click)
        {
            items.Add(click);
            return this;
        }
    }
}
