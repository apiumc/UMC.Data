using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web
{

    public class UIHeader : UMC.Data.IJSON
    {


        /// <summary>
        /// 肖像模式
        /// </summary>
        public class Portrait : UMC.Data.IJSON
        {
            WebMeta meta = new WebMeta();
            public Portrait(String src)
            {
                meta.Put("src", src);

            }

            public Portrait()
            {

            }
            public Portrait Key(String Key)
            {
                meta.Put("key", Key);
                return this;

            }
            public Portrait Click(UIClick click)
            {

                meta.Put("click", click);
                return this;
            }
            public Portrait Title(String title)
            {
                meta.Put("title", title);
                return this;
            }
            public Portrait Time(String time)
            {
                meta.Put("time", time);
                return this;
            }

            public Portrait Value(String value)
            {
                meta.Put("value", value);
                return this;
            }
            public Portrait Desc(String desc)
            {
                meta.Put("desc", desc);
                return this;
            }
            void IJSON.Read(string key, object value)
            {

            }
            void IJSON.Write(System.IO.TextWriter writer)
            {
                UMC.Data.JSON.Serialize(this.meta, writer);

            }
            public Portrait Gradient(int startColor, int endColor)
            {

                if (startColor < 0x1000)
                {
                    meta.Put("startColor", String.Format("#{0:x3}", startColor));
                }
                else
                {
                    meta.Put("startColor", String.Format("#{0:x6}", startColor));
                }
                if (endColor < 0x1000)
                {
                    meta.Put("endColor", String.Format("#{0:x3}", endColor));
                }
                else
                {
                    meta.Put("endColor", String.Format("#{0:x6}", endColor));
                }
                return this;
            }
        }
        /// <summary>
        /// 小头像模式
        /// </summary>
        public class Profile : UMC.Data.IJSON
        {
            WebMeta meta = new WebMeta();
            public Profile(string name, String number, String src)
            {
                meta.Put("name", name);
                meta.Put("number", number);
                meta.Put("src", src);

            }
            public Profile(string name, String src)
            {
                meta.Put("name", name);
                meta.Put("src", src);

            }
            public Profile Account(string amount, String tip, String tag)
            {
                return Account(amount, tip, tag, null);
            }
            public Profile Account(UIClick click)
            {
                return Account(null, null, null, click);
            }
            public Profile Account(string amount, String tip, String tag, UIClick click)
            {
                var acount = new WebMeta().Put("amount", amount);
                if (String.IsNullOrEmpty(tip) == false)
                {
                    acount.Put("tag", tip);
                }

                if (String.IsNullOrEmpty(tip) == false)
                {
                    acount.Put("tag", tip);
                }

                if (click != null)
                {
                    acount.Put("click", click);
                }
                meta.Put("account", acount);
                return this;
            }
            public Profile Put(string name, object value)
            {
                this.meta.Put(name, value);
                return this;
            }
            public Profile Click(UIClick click)
            {

                this.meta.Put("click", click);
                return this;
            }
            void IJSON.Read(string key, object value)
            {

            }
            void IJSON.Write(System.IO.TextWriter writer)
            {
                UMC.Data.JSON.Serialize(this.meta, writer);

            }
            public Profile Gradient(int startColor, int endColor)
            {

                if (startColor < 0x1000)
                {
                    meta.Put("startColor", String.Format("#{0:x3}", startColor));
                }
                else
                {
                    meta.Put("startColor", String.Format("#{0:x6}", startColor));
                }
                if (endColor < 0x1000)
                {
                    meta.Put("endColor", String.Format("#{0:x3}", endColor));
                }
                else
                {
                    meta.Put("endColor", String.Format("#{0:x6}", endColor));
                }
                return this;
            }
            public void AddKey(params UIClick[] clicks)
            {
                if (clicks.Length > 0)
                    meta.Put("Keys", clicks);
            }
            public void AddKey(params string[] keys)
            {
                var Keys = new List<WebMeta>();
                for (int i = 0; i < keys.Length; i = i + 2)
                {
                    if (i + 1 < keys.Length)
                    {
                        Keys.Add(new WebMeta().Put("text", keys[i]).Put("value", keys[i + 1]));
                    }
                }
                meta.Put("Keys", Keys.ToArray());

            }
        }
        protected WebMeta meta = new WebMeta();
        public UIHeader Slider(System.Collections.IList sliders)
        {
            meta.Put("type", "Slider").Put("data", new WebMeta().Put("data", sliders));
            return this;
        }
        public UIHeader Slider(System.Data.DataTable sliders)
        {
            meta.Put("type", "Slider").Put("data", new WebMeta().Put("data", sliders));
            return this;
        }
        public UIHeader Slider(params WebMeta[] sliders)
        {
            meta.Put("type", "Slider").Put("data", new WebMeta().Put("data", sliders));
            return this;
        }
        public UIHeader AddProfile(Profile profile, string numberFormat, string amountFormat)
        {

            meta.Put("type", "Profile").Put("data", profile).Put("format", new WebMeta().Put("number", numberFormat).Put("amount", amountFormat));
            return this;
        }
        public UIHeader AddProfile(Profile profile)
        {

            meta.Put("type", "Profile").Put("data", profile).Put("format", new WebMeta().Put("number", "{number}").Put("amount", "{amount}"));
            return this;
        }
        public UIHeader SliderSquare(params WebMeta[] sliders)
        {
            meta.Put("type", "SliderSquare").Put("data", new WebMeta().Put("data", sliders));
            return this;
        }
        public UIHeader AddPortrait(Portrait discount)
        {
            meta.Put("type", "Portrait").Put("data", discount);
            return this;
        }

        public UIHeader Desc(WebMeta data, String format, UIStyle style)
        {

            this.meta.Put("type", "Desc").Put("style", style).Put("format", new WebMeta().Put("desc", format)).Put("data", data);
            return this;

        }
        public UIHeader Put(String key, object value)
        {
            meta.Put(key, value);
            return this;
        }
        public UIHeader Search(String placeholder)
        {

            meta.Put("type", "Search").Put("data", new WebMeta().Put("placeholder", placeholder));

            return this;
        }
        public UIHeader Search(String placeholder, String Keyword)
        {

            meta.Put("type", "Search").Put("data", new WebMeta().Put("Keyword", Keyword)).PlaceHolder(placeholder);

            return this;
        }
        public UIHeader Coustom(UIView coustomCell)
        {
            var webm = new WebMeta();
            if (coustomCell.Src != null)
            {
                webm.Put("style", coustomCell.Style).Put("items", coustomCell.items).Put("src", coustomCell.Src.AbsoluteUri);//
            }
            else
            {
                webm.Put("style", coustomCell.Style).Put("items", coustomCell.items);
            }
            meta.Put("type", "Custom").Put("data", webm);

            return this;
        }

        void IJSON.Read(string key, object value)
        {

        }
        void IJSON.Write(System.IO.TextWriter writer)
        {
            UMC.Data.JSON.Serialize(this.meta, writer);

        }
    }
}
