using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIItemsTitleDesc : UICell
    {

        List<WebMeta> items = new List<WebMeta>();
        public UIItemsTitleDesc()
        {
            this.data = new WebMeta().Put("items", this.items); ;
            this.Type = "ItemsTitleDesc";
        }
        public UIItemsTitleDesc Add(String src, string title, string desc)
        {
            this.items.Add(new WebMeta().Put("title", title, "desc", desc).Put("src", src));
            this.Hide(4 | 8);
            return this;
        }
        public UIItemsTitleDesc HideTitle()
        {
            return Hide(1);
        }
        public UIItemsTitleDesc HideDesc()
        {
            return Hide(2);
        }
        public UIItemsTitleDesc HideLeft()
        {
            return Hide(4);
        }
        public UIItemsTitleDesc HideRight()
        {
            return Hide(8);
        }
        public UIItemsTitleDesc Hide(int hide)
        {
            int show = Utility.IntParse(this.data["show"] ?? "0", 0);
            this.data.Put("show", (hide | show).ToString());
            return this;
        }
        public UIItemsTitleDesc Add(String src, string title)
        {
            this.items.Add(new WebMeta().Put("title", title).Put("src", src));
            this.Hide(2 | 4 | 8);
            return this;
        }
        public UIItemsTitleDesc Add(UIClick click, String src, string title)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("title", title).Put("src", src));
            this.Hide(2 | 4 | 8);
            return this;
        }
        public UIItemsTitleDesc Add(String src, string title, string desc, string left, string right)
        {
            this.items.Add(new WebMeta().Put("title", title, "desc", desc).Put("src", src).Put("left", left).Put("right", right));
            return this;
        }
        public UIItemsTitleDesc Add(UIClick click, String src, string title, string desc)
        {
            this.Hide(4 | 8);
            this.items.Add(new WebMeta().Put("click", click).Put("title", title, "desc", desc).Put("src", src));
            return this;
        }
        public UIItemsTitleDesc Add(UIClick click, String src, string title, string desc, string left, string right)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("src", src).Put("title", title, "desc", desc).Put("left", left).Put("right", right)); ;//.Put("startColor", UIStyle.ToColor(startColor)).Put("endColor", UIStyle.ToColor(endColor)));
            return this;
        }
        private WebMeta data;
        public override object Data => data;

    }
}
