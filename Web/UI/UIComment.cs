using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIComment : UICell
    {
        public class Image
        {
            public string max
            {
                get; set;
            }
            public string src
            {
                get; set;
            }
        }
        public class Reply : UMC.Data.IJSON
        {
            public string title
            {
                get; set;
            }
            public string content
            {
                get; set;
            }
            public UIStyle style
            {
                get; set;

            }
            public WebMeta data
            {
                get; set;
            }

            void IJSON.Read(string key, object value)
            {

            }

            void IJSON.Write(System.IO.TextWriter writer)
            {
                UMC.Data.JSON.Serialize(new WebMeta().Put("format", new WebMeta().Put("content", this.content, "title", this.title)).Put("value", this.data).Put("style", this.style), writer);
            }
        }
        WebMeta data;
        public override object Data => data;
        public UIComment(string src)
        {
            this.data = new WebMeta().Put("src", src);
            this.Type = "Comment";
        }
        public UIComment Name(string name, string value)
        {

            this.data.Put(name, value);
            return this;
        }
        public UIComment ImageClick(UIClick click)
        {
            this.data.Put("image-click", click);
            return this;
        }
        public UIComment Desc(string desc)
        {

            this.data.Put("desc", desc);
            return this;
        }
        public UIComment Name(string name)
        {
            this.Format.Put("name", name);
            return this;
        }
        public UIComment Time(string title)
        {
            this.Format.Put("time", title);
            return this;
        }
        public UIComment Content(string content)
        {
            this.Format.Put("content", content);
            return this;
        }
        public UIComment Images(params Image[] images)
        {
            this.data.Put("image", images);
            return this;

        }
        public string Id
        {
            get; set;
        }
        public UIComment Replys(params Reply[] replys)
        {
            this.data.Put("replys", replys);
            return this;

        }
        public UIComment Tag(UIEventText tag)
        {
            this.data.Put("tag", tag);
            return this;
        }
        public UIComment Button(params UIEventText[] btns)
        {
            this.data.Put("buttons", btns);
            return this;

        }
    }
}
