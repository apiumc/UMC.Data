using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UMC.Data;

namespace UMC.Web
{

    public abstract class UICell : Data.IJSON
    {
        private class UICeller : UICell
        {
            WebMeta data;
            public override object Data => data;
            public UICeller(WebMeta data)
            {
                this.data = data;
            }
        }

        public static UICell Create(String type, WebMeta data)
        {
            var celler = new UICeller(data);
            celler.Type = type;
            return celler;
        }

        void IJSON.Write(TextWriter writer)
        {
            if (String.IsNullOrEmpty(this.Type))
            {
                throw new ArgumentException("Cell Type is empty");
            }
            writer.Write("{\"_CellName\":"); 
            UMC.Data.JSON.Serialize(this.Type, writer);
            writer.Write(",");
            UMC.Data.JSON.Serialize("value", writer);
            writer.Write(":");
            UMC.Data.JSON.Serialize(this.Data, writer);
            if (this._format.Count > 0)
            {

                writer.Write(",");
                UMC.Data.JSON.Serialize("format", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(_format, writer);
            }
            if (this._style.Count > 0)
            {

                writer.Write(",");
                UMC.Data.JSON.Serialize("style", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(_style, writer);
            }

            writer.Write("}");
        }

        void IJSON.Read(string key, object value)
        {

        }

        String _type;
        public string Type
        {
            get
            {
                return _type;

            }
            protected set
            {
                _type = value;
            }
        }
        WebMeta _format = new WebMeta();
        public WebMeta Format
        {
            get
            {
                return _format;

            }
        }

        UIStyle _style = new UIStyle();


        public UIStyle Style
        {
            get
            {
                return _style;

            }
        }
        public abstract object Data
        {
            get;
        }
    }
}
