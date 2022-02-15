using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UMC.Data;
using UMC.Web.UI;

namespace UMC.Web
{





    public class UISection : UMC.Data.IJSON
    {

        public class Editer
        {
            UMC.Web.WebMeta webMeta = new WebMeta();
            public Editer(int section, int row)
            {

                webMeta.Put("section", section).Put("row", row);

            }
            public Editer(String section, String row, string ui)
            {

                webMeta.Put("section", Utility.IntParse(section, 0)).Put("row", Utility.IntParse(row, 0)).Put("ui", ui);
            }
            public Editer(String section, String row)
            {

                webMeta.Put("section", Utility.IntParse(section, 0)).Put("row", Utility.IntParse(row, 0));

            }
            public Editer Put(UICell value, bool reloadSinge)
            {
                if (reloadSinge)
                {
                    webMeta.Put("value", new WebMeta().Cell(value)).Put("method", "PUT").Put("reloadSinle", true);
                }
                else
                {
                    webMeta.Put("value", new WebMeta().Cell(value)).Put("method", "PUT");//.Put("reloadSinle", true);

                }
                return this;
            }
            public Editer ReloadSinle()
            {
                webMeta.Put("reloadSinle", true);
                return this;
            }

            public Editer Value(WebMeta value)
            {
                webMeta.Put("value", value).Put("method", "VALUE").Put("reloadSinle", true); ;
                return this;
            }
            public Editer Delete()
            {
                webMeta.Put("method", "DEL");//.Put("reloadSinle", true);
                return this;
            }
            public Editer Append(UICell value)
            {
                webMeta.Put("value", new WebMeta().Cell(value)).Put("method", "APPEND");
                return this;
            }
            public Editer Insert(UICell value)
            {
                webMeta.Put("value", new WebMeta().Cell(value)).Put("method", "INSERT");
                return this;
            }
            public void Builder(WebContext context, String ui, bool endResponse)
            {
                context.Send(new UMC.Web.WebMeta().UIEvent("UI.Edit", ui, webMeta), endResponse);

            }
        }
        WebMeta _header = new WebMeta();
        private UISection()
        {

        }
        public WebMeta Header
        {
            get
            {
                return _header;
            }
        }
        UIHeader _uiheaders;
        UIHeader _uifooter;
        UIFootBar _footBar;
        UITitle _title;
        public static UISection Create(UIHeader header, UIFootBar footer)
        {
            var t = new UISection();
            t.Sections = new List<UISection>();
            t._componens = new List<UIView>();
            t.Sections.Add(t);
            WebMeta meta = new WebMeta();
            t._uiheaders = header;
            t._footBar = footer;
            return t;
        }


        public void Builder(WebContext context)
        {
            (this.parent ?? this).isOutPageKey = true;
            context.Response.ClientEvent |= WebEvent.DataEvent;
            context.Response.Headers.Put("DataEvent", this);
            context.End();

        }
        public static UISection Create(UIHeader header)
        {
            return Create(header, null, null);
        }
        public UITitle Title
        {
            set
            {
                var p = this.parent ?? this;
                p._title = value;
            }
            get
            {
                var p = this.parent ?? this;
                return p._title;
            }
        }
        public UIHeader UIHeader
        {
            set
            {
                var p = this.parent ?? this;
                p._uiheaders = value;
            }
            get
            {
                var p = this.parent ?? this;
                return p._uiheaders;
            }

        }
        public UIFootBar UIFootBar
        {
            set
            {
                var p = this.parent ?? this;
                p._footBar = value;
            }
            get
            {
                var p = this.parent ?? this;
                return p._footBar;
            }

        }
        List<UIView> _componens;
        public List<UIView> Componen
        {
            get
            {
                var p = this.parent ?? this;
                return p._componens;
            }
        }

        public UIHeader UIFooter
        {
            set
            {
                var p = this.parent ?? this;
                p._uifooter = value;
            }
            get
            {
                var p = this.parent ?? this;
                return p._uifooter;
            }
        }
        public static UISection Create(UIHeader header, UIFootBar footer, UITitle title)
        {
            var t = new UISection();
            t.Sections = new List<UISection>();
            t._componens = new List<UIView>();
            t.Sections.Add(t);
            WebMeta meta = new WebMeta();
            t._uiheaders = header;
            t._footBar = footer;
            t._title = title;
            return t;
        }
        public static UISection Create(UITitle title, UIFootBar footer)
        {
            var t = new UISection();
            t.Sections = new List<UISection>();
            t._componens = new List<UIView>();
            t.Sections.Add(t);
            WebMeta meta = new WebMeta();
            t._footBar = footer;
            t._title = title;
            return t;
        }
        public static UISection Create(UITitle title)
        {
            var t = new UISection();
            t.Sections = new List<UISection>();
            t._componens = new List<UIView>();
            t.Sections.Add(t);
            t._title = title;
            WebMeta meta = new WebMeta();
            return t;


        }
        public static UISection Create(UIHeader header, UITitle title)
        {
            var t = new UISection();
            t.Sections = new List<UISection>();
            t._componens = new List<UIView>();
            t.Sections.Add(t);
            t._title = title;
            WebMeta meta = new WebMeta();
            t._uiheaders = header;
            return t;


        }
        int _Total;
        public int Total
        {
            get
            {
                var p = this.parent ?? this;
                return p._Total;
            }
            set
            {

                var p = this.parent ?? this;
                p._Total = value;
            }
        }
        public static UISection Create()
        {
            var t = new UISection();
            t.Sections = new List<UISection>();
            t._componens = new List<UIView>();
            t.Sections.Add(t);
            return t;

        }
        public String Key
        {
            get;
            set;
        }

        public bool IsEditer
        {
            get;
            set;
        }
        List<UISection> Sections;
        //Object _data;
        //List<WebMeta> data = new List<WebMeta>();
        ArrayList data = new ArrayList();
        private UISection parent;

        public UISection NewSection()
        {
            var t = new UISection();
            t.parent = this.parent ?? this;
            t.Sections = this.Sections;
            //  t._header=
            this.Sections.Add(t);
            return t;
        }
        public int Count
        {
            get
            {
                return this.Sections.Count;
            }
        }
        public UISection NewSection(Array data)
        {
            var t = new UISection();
            t.Sections = this.Sections;
            t.parent = this.parent ?? this;
            this.Sections.Add(t);
            t.data.AddRange(data);
            return t;
        }
        public UISection AddCells(params WebMeta[] data)
        {

            this.data.AddRange(data);
            return this;
        }
        public UISection AddCells(params UICell[] cells)
        {
            foreach (var e in cells)
                this.Add(e);
            return this;
        }
        public int Length
        {
            get
            {
                return data.Count;
            }
        }
        public UISection AddCell(String text, String value, UIClick click)
        {
            return this.Add(UICell.Create("Cell", new WebMeta().Put("value", value, "text", text).Put("click", click)));

        }
        public UISection AddCell(String text, String value, string click)
        {
            return this.Add(UICell.Create("Cell", new WebMeta().Put("value", value, "text", text).Put("click", click)));

        }
        public UISection AddCell(String text, UIClick click)
        {
            return this.Add(UICell.Create("Cell", new WebMeta().Put("text", text).Put("click", click)));

        }
        public UISection AddCell(String text, String value)
        {
            return this.Add(UICell.Create("Cell", new WebMeta().Put("value", value, "text", text)));

        }
        public UISection AddCell(char icon, String text, String value)
        {
            return this.Add(UICell.Create("UI", new WebMeta().Put("value", value, "text", text).Put("Icon", icon)));

        }
        public UISection AddCell(char icon, String text, String value, UIClick click)
        {
            return this.Add(UICell.Create("UI", new WebMeta().Put("value", value, "text", text).Put("Icon", icon).Put("click", click)));

        }
        public UISection Add(UICell cell)
        {

            data.Add(cell);// new WebMeta().Put("_CellName", cell.Type).Put("value", cell.Data).Put("format", cell.Format).Put("style", cell.Style));
            return this;
        }
        public UISection Delete(UICell cell, UIEventText eventText)
        {
            data.Add(new WebMeta().Put("del", eventText).Put("_CellName", cell.Type).Put("value", cell.Data).Put("format", cell.Format).Put("style", cell.Style));
            return this;
        }

        public UISection DisableSeparatorLine()
        {
            disableSeparatorLine = true;
            return this;
        }
        bool disableSeparatorLine;
        public UISection Add(String type, WebMeta value, WebMeta format, UIStyle style)
        {
            data.Add(new WebMeta().Put("_CellName", type).Put("value", value).Put("format", format).Put("style", style));
            return this;
        }

        public UISection Add(String type, WebMeta value)
        {
            data.Add(new WebMeta().Put("_CellName", type).Put("value", value));
            return this;
        }

        void IJSON.Read(string key, object value)
        {
        }
        bool isOutPageKey;

        bool? _IsNext;
        public bool? IsNext
        {
            get
            {
                var p = this.parent ?? this;
                return p._IsNext;
            }
            set
            {

                var p = this.parent ?? this;
                p._IsNext = value;
            }
        }
        int? _StartIndex;
        public int? StartIndex
        {
            get
            {
                var p = this.parent ?? this;
                return p._StartIndex;
            }
            set
            {

                var p = this.parent ?? this;
                p._StartIndex = value;
            }
        }
        public String StartKey
        {
            get
            {
                var p = this.parent ?? this;
                return p._startKey;
            }
            set
            {

                var p = this.parent ?? this;
                p._startKey = value;
            }
        }
        string _startKey;

        void IJSON.Write(System.IO.TextWriter writer)
        {
            writer.Write("{");
            if ((this.parent ?? this).isOutPageKey)
            {
                UMC.Data.JSON.Serialize("type", writer); writer.Write(":");
                UMC.Data.JSON.Serialize("Pager", writer);
                writer.Write(",");

            }
            if (this.Componen.Count > 0)
            {
                UMC.Data.JSON.Serialize("Componen", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.Componen, writer);
                writer.Write(",");

            }
            if (this.UIHeader != null)
            {
                UMC.Data.JSON.Serialize("Header", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.UIHeader, writer);
                writer.Write(",");
            }
            if (this.Title != null)
            {
                UMC.Data.JSON.Serialize("Title", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(this.Title, writer);

                writer.Write(",");

            }
            if (this.UIFootBar != null)
            {
                UMC.Data.JSON.Serialize("FootBar", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.UIFootBar, writer);

                writer.Write(",");
            }
            if (this.UIFooter != null)
            {
                UMC.Data.JSON.Serialize("Footer", writer); writer.Write(":");
                UMC.Data.JSON.Serialize(this.UIFooter, writer);

                writer.Write(",");
            }
            if (Total > 0)
            {
                UMC.Data.JSON.Serialize("total", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(Total, writer);
                writer.Write(",");

            }
            if (String.IsNullOrEmpty(StartKey) == false)
            {

                UMC.Data.JSON.Serialize("startKey", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(StartKey, writer);
                writer.Write(",");
            }
            if (StartIndex.HasValue && StartIndex.Value > -1)
            {
                UMC.Data.JSON.Serialize("start", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(this.StartIndex.Value, writer);
                writer.Write(",");

            }
            if (this.IsNext.HasValue)
            {

                UMC.Data.JSON.Serialize("next", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(this.IsNext.Value, writer);
                writer.Write(",");
            }
            UMC.Data.JSON.Serialize("DataSource", writer);
            writer.Write(":[");
            bool b = false;
            foreach (var sec in this.Sections)
            {
                if (b)
                {
                    writer.Write(",");
                }
                else
                {
                    b = true;
                }
                writer.Write("{");
                if (String.IsNullOrEmpty(sec.Key) == false)
                {
                    UMC.Data.JSON.Serialize("key", writer);
                    writer.Write(":");
                    UMC.Data.JSON.Serialize(sec.Key, writer);
                    writer.Write(",");
                }
                if (sec.IsEditer)
                {
                    UMC.Data.JSON.Serialize("isEditer", writer);
                    writer.Write(":");
                    UMC.Data.JSON.Serialize(sec.IsEditer, writer);
                    writer.Write(",");

                }
                UMC.Data.JSON.Serialize("data", writer);
                writer.Write(":");
                UMC.Data.JSON.Serialize(sec.data, writer);

                if (sec.disableSeparatorLine)
                {
                    writer.Write(",");
                    UMC.Data.JSON.Serialize("separatorLine", writer);
                    writer.Write(":false");
                }
                if (sec._header.Count > 0)
                {
                    writer.Write(",");
                    UMC.Data.JSON.Serialize("header", writer); writer.Write(":");
                    UMC.Data.JSON.Serialize(sec._header, writer);
                }
                writer.Write("}");

            }

            writer.Write("]}");

        }
    }
}