using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UITitleMore : UICell
    {


        WebMeta data;
        public override object Data => data;
        public UITitleMore(string title)
        {
            this.data = new WebMeta().Put("title", title);
            this.Type = "TitleMore";
        }

        public UITitleMore Title(String title)
        {
            this.Format.Put("title", title);
            return this;
        }
        public UITitleMore More(String more)
        {
            this.Format.Put("more", more);
            return this;
        }
        public UITitleMore Click(UIClick click)
        {
            this.data.Put("click", click);
            this.data.Put("more", '\uE905');
            this.Style.Name("more", new UIStyle().Font("wdk").Size(12));
            return this;
        }
    }
}
