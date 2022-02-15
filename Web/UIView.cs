using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UMC.Data;

namespace UMC.Web
{

    public class UIView : UMC.Data.IJSON
    {

        String _type;
        internal List<Object> items = new List<object>();

        public UIView Add(Uri src, UIStyle style)
        {
            items.Add(new WebMeta().Put("src", src).Put("style", style));
            return this;

        }
        public UIView(String cellName)
        {
            if (cellName.StartsWith("UMC_") == false)
            {
                throw new ArgumentException("cellName需要以“UMC_”为开头");
            }
            this._type = cellName;
        }
        public UIView()
        {

        }
        public UIView Add(string key, Uri src, UIStyle style)
        {
            items.Add(new WebMeta().Put("src", src).Put("style", style).Put("key", key));
            return this;

        }
        public UIView Add(UIClick click, Uri src, UIStyle style)
        {
            items.Add(new WebMeta().Put("src", src).Put("style", style).Put("click", click));
            return this;

        }
        public UIView Add(string key, string text, UIStyle style)
        {
            items.Add(new WebMeta().Put("format", text).Put("style", style).Put("key", key));
            return this;
        }
        public UIView Add(UIClick click, string text, UIStyle style)
        {
            items.Add(new WebMeta().Put("format", text).Put("style", style).Put("click", click));
            return this;
        }
        public UIView Add(string text, UIStyle style)
        {
            items.Add(new WebMeta().Put("format", text).Put("style", style));
            return this;

        }
        public Uri Src
        {
            get; set;
        }
        public string Key
        {
            get; set;
        }

        void IJSON.Write(TextWriter writer)
        {

            if (String.IsNullOrEmpty(_type))
            {
                var webm = new WebMeta();
                if (this.Src != null)
                {
                    webm.Put("style", _style).Put("items", this.items).Put("src", this.Src.AbsoluteUri).Put("key", this.Key);//
                }
                else
                {
                    webm.Put("style", _style).Put("items", this.items);//
                }

                UMC.Data.JSON.Serialize(webm, writer);
            }
            else
            {
                writer.Write("{");
                UMC.Data.JSON.Serialize("key", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(this._type, writer);


                writer.Write(",");

                UMC.Data.JSON.Serialize("data", writer);
                writer.Write(":");
                var webm = new WebMeta();
                if (this.Src != null)
                {
                    webm.Put("style", _style).Put("items", this.items).Put("src", this.Src.AbsoluteUri).Put("key", this.Key);//
                }
                else
                {
                    webm.Put("style", _style).Put("items", this.items);//
                }

                UMC.Data.JSON.Serialize(webm, writer);



                writer.Write("}");
            }
        }


        void IJSON.Read(string key, object value)
        {

        }

        private UIStyle _style = new UIStyle();


        public UIStyle Style
        {
            get
            {
                return _style;

            }
        }
    }
}
