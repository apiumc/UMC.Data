using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UMC.Web.UI
{
    /// <summary>
    /// 优惠券
    /// </summary>
    public class UIDiscount : UICell
    {

        WebMeta data;
        public override object Data => data;
        private UIDiscount(WebMeta data)
        {
            this.data = data;
        }

        public static UIDiscount Create()
        {

            var t = new UIDiscount(new WebMeta());
            t.Type = "Discount";
            return t;
        }
        public UIDiscount(UIClick click)
        {
            //var t = Create();
            data = new WebMeta();
            data.Put("click", click);
            this.Type = "Discount";
            //return t;
        }
        public UIDiscount Click(UIClick click)
        {

            data.Put("click", click);
            return this;
        }

        public UIDiscount Gradient(int startColor, int endColor)
        {
            this.Style.Name("startColor", UIStyle.ToColor(startColor));
            this.Style.Name("endColor", UIStyle.ToColor(endColor));

           
            return this;
        }
        public UIDiscount Desc(string desc)
        {
            this.Format.Put("desc", desc);
            return this;
        }
        public UIDiscount Title(string title)
        {
            this.Format.Put("title", title);
            return this;

        }
        public UIDiscount End(string end)
        {
            data.Put("end", end);
            return this;

        }
        public UIDiscount Start(string start)
        {
            data.Put("start", start);
            return this;

        }
        public UIDiscount Value(string value)
        {
            this.Format.Put("value", value);
            return this;

        }
        public UIDiscount Time(string time)
        {
            this.Format.Put("time", time);
            return this;

        }
        public UIDiscount State(string state)
        {
            this.data.Put("state", state);
            return this;

        }
    }
}
