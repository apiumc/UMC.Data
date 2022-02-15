using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web
{

    public class UIFootBar : IJSON
    {
        protected WebMeta meta = new WebMeta();
        void IJSON.Read(string key, object value)
        {
            
        }
        void IJSON.Write(System.IO.TextWriter writer)
        {
            UMC.Data.JSON.Serialize(this.meta, writer);

        }
        List<object> _icons = new List<object>();
        List<object> _btons = new List<object>();
        public UIFootBar AddCart()
        {
            _icons.Add("-");
            if (this.meta.ContainsKey("icons") == false)
            {
                this.meta.Put("icons", _icons);
            }
            return this;
        }
        public UIFootBar AddText(params UIEventText[] text)
        {
            _btons.AddRange(text);
            if (this.meta.ContainsKey("buttons") == false)
            {
                this.meta.Put("buttons", _btons);
            }
            return this;

        }
        public UIFootBar AddIcon(params UIEventText[] icons)
        {
            _icons.AddRange(icons);
            if (this.meta.ContainsKey("icons") == false)
            {
                this.meta.Put("icons", _icons);
            }
            return this;

        }

        public bool IsFixed
        {
            get
            {
                return this.meta.ContainsKey("fixed");
            }
            set
            {
                if (value)
                {
                    this.meta.Put("fixed", true);
                }
                else
                {
                    this.meta.Remove("fixed");

                }
            }
        }
    }
}
