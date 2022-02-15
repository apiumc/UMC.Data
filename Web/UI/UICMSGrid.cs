using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{


    public class UICMSGrid : UICell
    {
        public class Cell : IJSON
        {
            public WebMeta Data
            {
                get; set;
            }
            public UIStyle Style
            {
                get; set;
            }
            public string Format
            {
                get; set;
            }
            public String Text
            {
                get; set;
            }

            void IJSON.Read(string key, object value)
            {

            }

            void IJSON.Write(TextWriter writer)
            {
                writer.Write("{");
                if (this.Data == null)
                {
                    writer.Write("\"text\":");
                    JSON.Serialize(this.Text ?? "", writer);

                }
                else
                {
                    writer.Write("\"data\":");
                    JSON.Serialize(this.Data ?? new WebMeta(), writer);
                }
                if (String.IsNullOrEmpty(this.Format) == false)
                {
                    writer.Write(',');
                    writer.Write("\"format\":");
                    JSON.Serialize(this.Format ?? "{text}", writer);
                }
                if (this.Style != null)
                {

                    writer.Write(',');
                    writer.Write("\"style\":");
                    JSON.Serialize(this.Style, writer);
                }
                writer.Write('}');
            }
        }
        List<Cell[]> cells = new List<Cell[]>();
        public UICMSGrid()
        {
            this.data = new WebMeta().Put("grid", this.cells);
            this.Type = "CMSGrid";
        }
        private WebMeta data;
        public override object Data => data;

        public UICMSGrid AddRow(params Cell[] cells)
        {
            this.cells.Add(cells);

            return this;
        }
        public UICMSGrid AddRow(params String[] cells)
        {
            var cls = new List<Cell>();
            foreach (var s in cells)
            {
                cls.Add(new Cell() { Text = s });
            }
            this.cells.Add(cls.ToArray());

            return this;
        }
        public int Count
        {
            get
            {
                return this.cells.Count;
            }
        }
    }

}
