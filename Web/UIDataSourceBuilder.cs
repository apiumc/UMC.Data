using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UMC.Web
{
     
    public class UIDataSourceBuilder
    {

        WebMeta _data = new UMC.Web.WebMeta();

        public UIDataSourceBuilder Menu(params Web.UIClick[] clicks)
        {
            _data.Put("menu", clicks);
            return this;
        }
        public string Title
        {
            get; set;
        }
        public UIDataSourceBuilder RefreshEvent(params String[] eventName)
        {

            _data.Put("RefreshEvent", String.Join(",", eventName));
            return this;
        }
        public UIDataSourceBuilder CloseEvent(params String[] eventName)
        {

            _data.Put("CloseEvent", String.Join(",", eventName));
            return this;
        }
        public UIDataSourceBuilder Header(UIHeader header)
        {
            _data.Put("Header", header);
            return this;
        }
        public UIDataSourceBuilder Footer(UIHeader footer)
        {
            _data.Put("Footer", footer);
            return this;
        }

        public WebMeta Builder(params UIDataSource[] dataSources)
        {
            return new UMC.Web.WebMeta(_data.GetDictionary()).Put("type", "DataSource").Put("DataSource", dataSources).Put("title", this.Title);//.Put("model");
        }
        public WebMeta BinderCells(params UIDataSource[] dataSources)
        {

            return new UMC.Web.WebMeta(_data.GetDictionary()).Put("type", "DataSource").Put("DataSource", dataSources).Put("model", "Cells").Put("title", this.Title);
        }
    }
}
