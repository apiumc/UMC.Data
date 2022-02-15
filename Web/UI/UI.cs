using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UI : UICell
    {

        public UI(WebMeta data)
        {
            this.data = data;
            this.Type = "UI";
        }
        public UI(string text, string value) : this(text, value, false)
        {

        }
        public UI(string text, string value, bool isForm)
        {
            this.data = new WebMeta().Put("text", text, "value", value);
            this.Type = isForm ? "Cell" : "UI";
        }
        WebMeta data;
        public override object Data => data;

        public UI Text(String text)
        {
            this.Format.Put("text", text);
            return this;
        }
        public UI Value(String value)
        {
            this.Format.Put("value", value);
            return this;
        }
        public UI Click(UIClick click)
        {
            data.Put("click", click);
            return this;

        }
        public UI Icon(char icon)
        {
            data.Put("Icon", icon);
            return this;

        }
        public UI Icon(char icon, int color)
        {
            data.Put("Icon", icon);
            this.Style.Name("Icon").Color(color);
            return this;

        }
    }
}
