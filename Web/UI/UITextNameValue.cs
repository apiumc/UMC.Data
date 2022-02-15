
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
namespace UMC.Web.UI
{
    
    public class UITextNameValue : UICell
    {
        public UITextNameValue(String name, string text, string value)
        {
            this.data = new WebMeta().Put("name", name, "text", text, "value", value);
            this.Type = "TextNameValue";
        }
        public UITextNameValue Click(UIClick click)
        {

            this.data.Put("click", click);//name
            return this;
        }
        public UITextNameValue(WebMeta desc)
        {
            this.data = desc;
            this.Type = "TextNameValue";

        }
        public UITextNameValue Name(String name)
        {
            this.Format.Put("name", name);
            return this;
        }
        public UITextNameValue Text(String text)
        {
            this.Format.Put("text", text);
            return this;
        }
        public UITextNameValue Value(String value)
        {
            this.Format.Put("value", value);
            return this;
        }
        WebMeta data;
        public override object Data => data;

    }








}