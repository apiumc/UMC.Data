using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UMC.Web.UI
{
    public class UIImageTitleBottom : UICell
    {

        public UIImageTitleBottom(String src)
        {
            this.data = new WebMeta();
            this.data.Put("src", src);
            this.Type = "ImageTitleBottom";


        }
        private UIImageTitleBottom(WebMeta data)
        {
            this.data = data;
        }
        public UIImageTitleBottom(WebMeta data, String src)
        {
            this.data = data;
            this.data.Put("src", src);
            this.Type = "ImageTitleBottom";


        }
        public UIImageTitleBottom(UIClick click, WebMeta data, String src)
        {
            this.data = data;
            this.data.Put("src", src);
            this.data.Put("click", click);
            this.Type = "ImageTitleBottom";

        }
        public UIImageTitleBottom Src(String src)
        {
            this.data.Put("src", src);
            return this;
        }
        public UIImageTitleBottom Left(string desc)
        {
            Format.Put("left", desc);
            return this;
        }
        public UIImageTitleBottom Title(string desc)
        {
            this.Format.Put("title", desc);
            return this;
        }
        public UIImageTitleBottom Right(string price)
        {
            this.Format.Put("right", price);
            return this;
        }
        /// <summary>
        /// 采用商品价格格式化
        /// </summary>
        /// <param name="price">价格</param>
        /// <param name="unit">单位</param>
        /// <returns></returns>
        public UIImageTitleBottom Left(String price, String unit)
        {
            this.data.Put("price", price, "unit", unit);
            this.Format.Put("left", "¥{1:price}/{1:unit}");
            this.Style.Name("price", new UIStyle().Size(20).Color(0xdb3652)).Name("unit", new UIStyle().Size(15).Color(0x999)).Name("orgin", new UIStyle().Color(0x999).Size(12).DelLine());
            return this;
        }
        /// <summary>
        /// 采用商品价格格式化
        /// </summary>
        /// <param name="price">价格</param>
        /// <param name="orgin">原价</param>
        /// <param name="unit">单位</param>
        public UIImageTitleBottom Left(String price, String orgin, String unit)
        {
            this.data.Put("price", price, "unit", unit, "orgin", orgin);
            this.Format.Put("left", "¥{1:price}/{1:unit} 原价:{3:orgin}");
            this.Style.Name("price", new UIStyle().Size(14).Color(0xdb3652)).Name("unit", new UIStyle().Size(12).Color(0x999)).Name("orgin", new UIStyle().Color(0x999).Size(12).DelLine());
            return this;
        }
        public UIImageTitleBottom Click(UIClick click)
        {
            this.data.Put("click", click);
            return this;
        }
        WebMeta data;
        public override object Data => data;
    }
}
