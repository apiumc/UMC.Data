using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{


    public class UINineImage : UICell
    {

        List<WebMeta> items = new List<WebMeta>();
        public UINineImage()
        {
            this.data = new WebMeta().Put("images", this.items); ;
            this.Type = "NineImage";
        }
        public UINineImage Add(String src)
        {
            this.items.Add(new WebMeta().Put("src", src));
            return this;
        }
        public UINineImage Add(UIClick click, String src)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("src", src));
            return this;
        }
        public UINineImage Click(UIClick click)
        {
            this.data.Put("click", click);//.Put("src", src));
            return this;
        }
        private WebMeta data;
        public override object Data => data;

    }
}

