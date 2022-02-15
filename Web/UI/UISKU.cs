using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UISKU : UICell
    {

        public UISKU(string title)
        {
            this.data = new WebMeta().Put("title", title);
            this.Type = "UISKU";
        }
        public UISKU()
        {


            this.data = new WebMeta();//.Put("title", title);
            this.Type = "UISKU";
        }
        WebMeta data;
        public override object Data => data;

    }
}
