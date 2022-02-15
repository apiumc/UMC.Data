using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{

    public class UIImageTextDescQuantity : UICell
    {
        public UIImageTextDescQuantity(string src, String title, string desc) : this(new WebMeta().Put("desc", desc, "title", title, "src", src))
        {
        }
        public UIImageTextDescQuantity ImageClick(UIClick click)
        {

            this.data.Put("image-click", click);
            return this;
        }
        /// <summary>
        /// 数据减少事件
        /// </summary>
        /// <param name="click"></param>
        /// <returns></returns>
        public UIImageTextDescQuantity DecreaseClick(UIClick click)
        {

            this.data.Put("decrease-click", click);
            return this;
        }
        /// <summary>
        /// 数据增加事件
        /// </summary>
        /// <param name="click"></param>
        /// <returns></returns>
        public UIImageTextDescQuantity IncreaseClick(UIClick click)
        {

            this.data.Put("increase-click", click);
            return this;
        }
        public UIImageTextDescQuantity() : this(new WebMeta())
        {

        }
        public UIImageTextDescQuantity(WebMeta data)
        {
            this.data = data;
            this.Type = "ImageTextDescQuantity";

        }
        public UIImageTextDescQuantity Quantity(int quantity)
        {
            this.data.Put("Quantity", quantity);
            return this;
        }
        public UIImageTextDescQuantity Src(string src)
        {
            this.data.Put("src", src);
            return this;
        }
        public UIImageTextDescQuantity Title(String title)
        {
            this.Format.Put("title", title);
            return this;
        }
        public UIImageTextDescQuantity Desc(String desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
        WebMeta data;
        public override object Data => data;
    }
}
