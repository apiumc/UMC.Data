using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UITextItems : UICell
    {

        List<UIEventText> items = new List<UIEventText>();
        public UITextItems()
        {
            this.data = new WebMeta().Put("data", this.items); ;
            this.Type = "TextItems";
        }
        public UITextItems(string model, String cmd)
        {
            this.data = new WebMeta().Put("model", model, "cmd", cmd).Put("data", this.items); ;
            this.Type = "TextItems";
        }
        public UITextItems(string model, String cmd, string value)
        {
            this.data = new WebMeta().Put("model", model, "cmd", cmd, "search", value).Put("data", this.items); ;
            this.Type = "TextItems";
        }
        public UITextItems(string model, String cmd, WebMeta value)
        {
            this.data = new WebMeta().Put("model", model, "cmd", cmd).Put("search", value).Put("data", this.items); ;
            this.Type = "TextItems";
        }
        public UITextItems Event(String key)
        {
            this.data.Put("event", key);
            return this;
        }

        public UITextItems Add(UIEventText item)
        {
            this.items.Add(item);
            return this;
        }
        public UITextItems Msg(string msg)
        {
            this.data.Put("msg", msg);
            return this;
        }
        public UITextItems Add(params UIEventText[] items)
        {
            this.items.AddRange(items);
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
