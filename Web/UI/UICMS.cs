using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UMC.Web.UI
{
    public class UICMS : UICell
    {
        WebMeta data;
        public override object Data => data;

        /// <summary>
        /// 创建无图资讯组件
        /// </summary>
        /// <param name="click"></param>
        /// <param name="data"></param>
        public UICMS(UIClick click, WebMeta data)
        {
            this.data = data;
            this.data.Put("click", click);
            this.Type = "CMSMax";

        }
        /// <summary>
        /// 创建大图资讯组件，并支持视频
        /// </summary>
        /// <param name="data"></param>
        /// <param name="click"></param>
        /// <param name="videoSrc"></param>
        /// <param name="src"></param>
        public UICMS(WebMeta data, UIClick click, Uri videoSrc, String src)
        {
            this.data = data.Put("click", click).Put("src", src).Put("video-src", videoSrc);
            this.Type = "CMSMax";
        }
        /// <summary>
        /// 创建视频资讯组件
        /// </summary>
        public UICMS(WebMeta data, Uri videoSrc, String src)
        {
            this.data = data.Put("src", src).Put("video-src", videoSrc);
            this.Type = "CMSMax";
        }
        /// <summary>
        /// 创建单图或者大图资讯组件
        /// </summary>
        /// <param name="click"></param>
        /// <param name="data"></param>
        /// <param name="src"></param>
        /// <param name="max">是否是大图</param>
        public UICMS(UIClick click, WebMeta data, String src, bool max)
        {
            this.data = data;
            this.data.Put("src", src);
            this.data.Put("click", click);
            this.Type = max ? "CMSMax" : "CMSOne";
        }
        /// <summary>
        /// 创建单图资讯组件
        /// </summary>
        /// <param name="click"></param>
        /// <param name="data"></param>
        /// <param name="src"></param>
        public UICMS(UIClick click, WebMeta data, String src)
        {
            this.data = data;// = new UICMS(data);
            this.data.Put("src", src);
            this.data.Put("click", click);
            this.Type = "CMSOne";

        }
        /// <summary>
        /// 创建三张图资讯组件
        /// </summary>
        /// <param name="click"></param>
        /// <param name="data"></param>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="src3"></param>
        public UICMS(UIClick click, WebMeta data, String src1, String src2, String src3)
        {
            this.data = data;
            this.data.Put("click", click);
            this.Type = "CMSThree";

            this.data.Put("images", new string[] { src1, src2, src3 });
        }
        public UICMS Desc(string desc)
        {
            Format.Put("desc", desc);
            return this;
        }
        public UICMS Title(string title)
        {
            this.Format.Put("title", title);
            return this;

        }
        public UICMS Right(string right)
        {
            this.Format.Put("right", right);
            return this;

        }
        public UICMS Left(string left)
        {
            this.Format.Put("left", left);
            return this;

        }
    }
}
