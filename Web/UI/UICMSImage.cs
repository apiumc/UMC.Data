using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UICMSImage : UICell
    {
        public UICMSImage(Uri video, String src)
        {
            this.data = (new WebMeta().Put("src", src).Put("video-src", video.AbsoluteUri));
            this.Type = "CMSImage";
        }
        public UICMSImage(WebMeta data)
        {
            this.data = data;
            this.Type = "CMSImage";
        }
        public UICMSImage(string src)
        {
            this.data = new WebMeta().Put("src", src);
            this.Type = "CMSImage";
        }
        WebMeta data;
        public override object Data => data;


    }
}
