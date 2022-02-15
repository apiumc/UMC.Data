using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UISlider : UICell
    {
        List<WebMeta> items = new List<WebMeta>();


        public UISlider()
        {
            this.data = new WebMeta().Put("data", this.items); ;
            this.Type = "Slider";
        }
        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }
        public UISlider(bool auto)
        {

            this.data = new WebMeta().Put("data", this.items); ;
            if (auto)
            {
                this.data.Put("auto", true);
            }
            this.Type = "Slider";
        }
        public UISlider Add(String src)
        {
            this.items.Add(new WebMeta().Put("src", src));
            return this;
        }
        public UISlider Add(Uri video, String src)
        {
            this.items.Add(new WebMeta().Put("src", src).Put("video-src", video.AbsoluteUri));
            return this;
        }
        public UISlider Add(UIClick click)
        {
            this.items.Add(new WebMeta().Put("click", click));//.Put("src", src));
            return this;

        }
        public UISlider Add(UIClick click, String src)
        {
            this.items.Add(new WebMeta().Put("click", click).Put("src", src));
            return this;
        }

        private WebMeta data;
        public override object Data => data;

        public UISlider Row()
        {
            this.data.Put("type", "Row");
            return this;
        }
        public UISlider Small()
        {
            this.data.Put("type", "Small");
            return this;
        }
        public UISlider Square()
        {
            this.data.Put("type", "Square");
            return this;
        }
    }
}
