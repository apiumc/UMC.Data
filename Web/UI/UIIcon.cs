using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIIcon : UICell
    {
        List<UIEventText> items = new List<UIEventText>();
        public UIIcon()
        {
            this.data = new WebMeta();
            this.data.Put("icons", items);
            this.Type = "Icons";

        }
        WebMeta data;
        public override object Data => data;

        public UIIcon Add(params UIEventText[] btns)
        {
            this.items.AddRange(btns);
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
