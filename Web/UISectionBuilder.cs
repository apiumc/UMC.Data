using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UMC.Web
{
     
    public class UISectionBuilder
    {
        WebMeta _data = new UMC.Web.WebMeta();
        public UISectionBuilder(String model, String cmd)
        {
            _data.Put("model", model, "cmd", cmd);//.Put("search", search);

        }
        public UISectionBuilder(String model, String cmd, WebMeta search)
        {
            _data.Put("model", model, "cmd", cmd).Put("search", search);
        }
        public UISectionBuilder RefreshEvent(params String[] eventName)
        {

            _data.Put("RefreshEvent", String.Join(",", eventName));
            return this;
        }
        public UISectionBuilder DataEvent(params String[] eventName)
        {

            _data.Put("DataEvent", String.Join(",", eventName));
            return this;
        }
        public UISectionBuilder Scanning(Web.UIClick click)
        {

            _data.Put("Scanning", click);
            return this;
        }
        public UISectionBuilder CloseEvent(params String[] eventName)
        {

            _data.Put("CloseEvent", String.Join(",", eventName));
            return this;
        }

        public WebMeta Builder()
        {
            return new UMC.Web.WebMeta(_data.GetDictionary()).Put("type", "Pager");//.Put("DataSource", dataSources).Put("title", this.Title);//.Put("model");
        }
        public WebMeta BuilderSheet()
        {
            return new UMC.Web.WebMeta(_data.GetDictionary()).Put("type", "Sheet");//.Put("DataSource", dataSources).Put("title", this.Title);//.Put("model");
        }
    }
     
}
