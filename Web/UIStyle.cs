using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Web
{

    public class UIStyle : UMC.Data.IJSON
    {
        public static int[] Padding(WebMeta meta)
        {
            return Padding(meta["Padding"]);
        }
        public static int[] Padding(String padding)
        {
            if (String.IsNullOrEmpty(padding) == false)
            {
                var ids = new List<int>();
                var ps = padding.Split(' ');
                switch (ps.Length)
                {
                    case 1:
                        var t = Data.Utility.IntParse(ps[0], 0);
                        ids.Add(t);
                        ids.Add(t);
                        ids.Add(t);
                        ids.Add(t);
                        break;
                    case 2:

                        var t21 = Data.Utility.IntParse(ps[0], 0);
                        var t22 = Data.Utility.IntParse(ps[1], 0);
                        ids.Add(t21);
                        ids.Add(t22);
                        ids.Add(t21);
                        ids.Add(t22);
                        break;
                    case 3:
                        var t31 = Data.Utility.IntParse(ps[0], 0);
                        var t32 = Data.Utility.IntParse(ps[1], 0);
                        var t33 = Data.Utility.IntParse(ps[1], 0);
                        ids.Add(t31);
                        ids.Add(t32);
                        ids.Add(t33);
                        ids.Add(0);
                        break;
                    default:
                        ids.Add(Data.Utility.IntParse(ps[0], 0));
                        ids.Add(Data.Utility.IntParse(ps[1], 0));
                        ids.Add(Data.Utility.IntParse(ps[2], 0));
                        ids.Add(Data.Utility.IntParse(ps[3], 0));
                        break;
                }
                return ids.ToArray();
            }
            return new int[0] { };
        }
        WebMeta meta = new WebMeta();

        public UIStyle AlignLeft()
        {
            return Align(0);

        }
        public UIStyle()
        {

        }
        public UIStyle(WebMeta meta)
        {
            if (meta != null)
            {
                this.meta = meta;
            }
        }
        /// <summary>
        /// 反转动画
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public UIStyle Reverse(float duration)
        {
            meta.Put("animation-name", "reverse").Put("animation-duration", duration);
            return this;
        }
        /// <summary>
        /// 默认反转动画
        /// </summary>
        public UIStyle Reverse()
        {
            meta.Put("animation-name", "reverse");
            return this;
        }
        public UIStyle Rotate(float duration)
        {
            meta.Put("animation-name", "rotate").Put("animation-duration", duration);
            return this;
        }
        public UIStyle Rotate()
        {
            meta.Put("animation-name", "rotate");
            return this;
        }
        public UIStyle Scale(float duration, float form, float to)
        {
            meta.Put("animation-name", "rotate").Put("animation-duration", duration).Put("animation-form", form).Put("animation-to", to);
            return this;
        }
        public UIStyle Scale()
        {
            meta.Put("animation-name", "scale");
            return this;
        }
        public void Copy(UIStyle style)
        {
            var dic = this.meta.GetDictionary();

            var em = style.meta.GetDictionary().GetEnumerator();
            while (em.MoveNext())
            {
                dic[em.Key] = em.Value;
            }

        }
        public UIStyle Height(int width)
        {
            meta.Put("height", width);
            return this;
        }
        public UIStyle Height(string width)
        {
            meta.Put("height", width);
            return this;
        }
        public UIStyle Width(int width)
        {
            meta.Put("width", width);
            return this;
        }
        public UIStyle Width(string width)
        {
            meta.Put("width", width);
            return this;
        }
        public UIStyle AlignCenter()
        {
            return Align(1);

        }
        public int Count
        {
            get
            {
                return meta.Count;
            }
        }
        public UIStyle Fixed()
        {
            meta.Put("fixed", true);
            return this;
        }
        public UIStyle Radius(int radius)
        {
            meta.Put("border-radius", radius);
            return this;
        }
        public UIStyle AlignRight()
        {
            return Align(2);

        }
        /// <summary>
        /// 文本对齐
        /// </summary>
        /// <param name="c">0为left,2为center,3为right</param>
        /// <returns></returns>
        public UIStyle Align(int c)
        {
            switch (c % 3)
            {
                default:
                    meta.Put("text-align", "left");
                    break;
                case 1:
                    meta.Put("text-align", "center");
                    break;
                case 2:
                    meta.Put("text-align", "right");
                    break;
            }
            return this;
        }
        public UIStyle Name(String key, string value)
        {
            meta.Put(key, value);
            return this;

        }
        public UIStyle Name(String key, int value)
        {

            meta.Put(key, value);
            return this;
        }
        public UIStyle Bold()
        {

            meta.Put("font-weight", "bold");
            return this;
        }
        public UIStyle Padding(params int[] padding)
        {

            switch (padding.Length)
            {
                case 0:
                    break;
                case 1:
                    meta.Put("padding", String.Format("{0} {0} {0} {0}", padding[0]));
                    break;
                case 2:
                case 3:
                    meta.Put("padding", String.Format("{0} {1} {0} {1}", padding[0], padding[1]));
                    break;
                case 4:
                    meta.Put("padding", String.Format("{0} {1} {2} {3}", padding[0], padding[1], padding[2], padding[3]));
                    break;
            }

            return this;
        }
        public UIStyle Font(string c)
        {
            meta.Put("font", c);
            return this;
        }
        public UIStyle Name(String key, UIStyle style)
        {
            meta.Put(key, style);
            return this;

        }
        public UIStyle Name(String key)
        {
            if (meta.ContainsKey(key))
            {
                var obj = meta.GetDictionary()[key];
                if(obj is UIStyle)
                {
                    return (UIStyle)obj;
                }
            }
           
            var style = new UIStyle();
            meta.Put(key, style);
            return style;

        }
        public UIStyle BgColor()
        {
            meta.Put("background-color", "highlighted");
            return this;
        }
        public UIStyle BgColor(int color)
        {

            meta.Put("background-color", ToColor(color));

            return this;
        }
        public static string ToColor(int color)
        {

            if (color < 0x1000)
            {
                return String.Format("#{0:x3}", color);
            }
            else
            {
                return String.Format("#{0:x6}", color);
            }
        }
        public UIStyle Color()
        {

            meta.Put("color", "highlighted");

            return this;
        }

        public UIStyle Color(int color)
        {

            meta.Put("color", ToColor(color));

            return this;
        }
        public UIStyle BorderColor(int color)
        {

            meta.Put("border-color", ToColor(color));

            return this;
        }
        public UIStyle BorderAll()
        {

            meta.Put("border", "all");

            return this;
        }
        public UIStyle BorderTop()
        {

            meta.Put("border", "top");

            return this;
        }
        public UIStyle BorderBottom()
        {

            meta.Put("border", "bottom");

            return this;
        }
        public UIStyle BorderNone()
        {

            meta.Put("border", "none");

            return this;
        }



        public UIStyle UnderLine()
        {

            meta.Put("text-decoration", "underline");
            return this;
        }
        public UIStyle DelLine()
        {

            meta.Put("text-decoration", "line-through");
            return this;
        }
        public UIStyle Size(int size)
        {
            meta.Put("font-size", size.ToString());
            return this;
        }
        public UIStyle Click(UIClick click)
        {
            meta.Put("click", click);
            return this;

        }
        void Data.IJSON.Read(string key, object value)
        {

        }
        void Data.IJSON.Write(System.IO.TextWriter writer)
        {
            UMC.Data.JSON.Serialize(this.meta, writer);

        }
    }
}
