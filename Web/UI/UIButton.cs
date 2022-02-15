using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIButton : UICell
    {
        
        public UIButton()
        {
            this.data = new WebMeta();
            this.Type = "UIButton";

        }
        WebMeta data;
        public override object Data => data;
        public UIButton(WebMeta data)
        {
            this.data = data;
            this.Type = "UIButton";
        }
        public void Title(String title)
        {
            this.Format.Put("title", title);
        }
        public void Button(params UIEventText[] btns)
        {
            data.Put("buttons", btns);
        }

    }
}
