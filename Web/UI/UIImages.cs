using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIImages : UICell
    {

        List<WebMeta> items = new List<WebMeta>();
        public UIImages()
        {
            this.data = new WebMeta().Put("data", this.items); ;
            this.Type = "UIImages";
        }
        public UIImages Add(String src)
        {
            this.items.Add(new WebMeta().Put("src", src));
            return this;
        }
        public UIImages Add(UIClick click, String src)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("src", src));
            return this;
        }

        private WebMeta data;
        public override object Data => data;

    }
}
