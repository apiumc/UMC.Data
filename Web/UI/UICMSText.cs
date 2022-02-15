using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UICMSText : UICell
    {

        public UICMSText(WebMeta data)
        {
            this.data = data;
            this.Type = "CMSText";
        }
        public UICMSText(string text)
        {
            this.data = new WebMeta().Put("text", text);//, "src", src);
            this.Type = "CMSText";
        }
        WebMeta data;
        public override object Data => data;

        /// <summary>
        /// 采用引用格式显示
        /// </summary>
        /// <returns></returns>
        public UICMSText Rel()
        {
            this.Type = "CMSRel";
            return this;
        }
        /// <summary>
        /// 采用代码格式显示
        /// </summary>
        /// <returns></returns>
        public UICMSText Code()
        {
            this.Type = "CMSCode";
            return this;
        }



    }
}
