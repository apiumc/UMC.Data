using System;
using System.Collections.Generic;
using System.Text;
using UMC.Data;

namespace UMC.Web.UI
{
    public class UIImageTitleDescBottom : UICell
    {
        private UIImageTitleDescBottom(WebMeta data)
        {
            this.data = data;
        }
        public UIImageTitleDescBottom(WebMeta data, String src)
        {
            this.data = data;
            this.data.Put("src", src);
            this.Type = "ImageTitleDescBottom";

        }
        WebMeta data;
        public override object Data => data;

        public UIImageTitleDescBottom Desc(string desc)
        {
            Format.Put("desc", desc);
            return this;

        }
        public UIImageTitleDescBottom Click(UIClick click)
        {
            this.data.Put("click", click);
            return this;
        }
        public UIImageTitleDescBottom Title(string desc)
        {

            this.Format.Put("title", desc);
            return this;

        }
        public UIImageTitleDescBottom Left(string left)
        {
            this.Format.Put("left", left);
            return this;

        }
        public UIImageTitleDescBottom Right(string right)
        {
            this.Format.Put("right", right);
            return this;

        }
        public UIImageTitleDescBottom HideTitle()
        {
            return Hide(1);
        }
        public UIImageTitleDescBottom HideDesc()
        {
            return Hide(2);
        }
        public UIImageTitleDescBottom HideLeft()
        {
            return Hide(4);
        }
        public UIImageTitleDescBottom HideRight()
        {
            return Hide(8);
        }
        public UIImageTitleDescBottom Hide(int hide)
        {
            int show = Utility.IntParse(this.data["show"] ?? "0", 0);
            this.data.Put("show", (hide | show).ToString());
            return this;
        }
    }
}
