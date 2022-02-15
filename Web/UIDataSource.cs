using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UMC.Web
{

    public class UIDataSource : UMC.Data.IJSON
    {
        Hashtable _data = new Hashtable();
        public UIDataSource(System.Data.DataTable data, String cell)
        {
            _data["data"] = data;
            if (String.IsNullOrEmpty(cell) == false)
            {
                _data["cell"] = cell;
            }
        }
        public UIDataSource(System.Data.DataTable data, UICell cell)
        {
            _data["data"] = data;

            _data["cell"] = cell.Type;
            _style.Copy(cell.Style);
            this.Format(cell.Format);

        }
        public UIDataSource(ICollection data, UICell cell)
        {
            _data["data"] = data;

            _data["cell"] = cell.Type;
            _style.Copy(cell.Style);
            this.Format(cell.Format);

        }
        public UIDataSource(ICollection data, string cell)
        {
            _data["data"] = data;
            if (String.IsNullOrEmpty(cell) == false)
            {
                _data["cell"] = cell;
            }
        }
        public string Text
        {
            get
            {
                return _data["text"] as string;
            }
            set
            {
                _data["text"] = value;
            }
        }
        public UIDataSource(string model, string cmd)
            : this(model, cmd, "")
        {
        }
        public UIDataSource(string model, string cmd, UICell cell)
        {
            if (String.IsNullOrEmpty(model) == false && String.IsNullOrEmpty(cmd) == false)
            {
                _data["model"] = model;
                _data["cmd"] = cmd;
            }
            _data["cell"] = cell.Type;
            _style.Copy(cell.Style);
            this.Format(cell.Format);

        }
        public UIDataSource(string model, string cmd, string cell)
        {
            if (String.IsNullOrEmpty(model) == false && String.IsNullOrEmpty(cmd) == false)
            {
                _data["model"] = model;
                _data["cmd"] = cmd;
            }
            if (String.IsNullOrEmpty(cell) == false)
            {
                _data["cell"] = cell;
            }
        }
        public UIDataSource(string model, string cmd, WebMeta search, UICell cell)
        {
            if (String.IsNullOrEmpty(model) == false && String.IsNullOrEmpty(cmd) == false)
            {
                _data["model"] = model;
                _data["cmd"] = cmd;
            }
            _data["cell"] = cell.Type;
            _style.Copy(cell.Style);
            this.Format(cell.Format);
            if (search != null && search.Count > 0)
            {
                _data["search"] = search;
            }
        }
        public UIDataSource(string model, string cmd, WebMeta search, String cell)
        {
            if (String.IsNullOrEmpty(model) == false && String.IsNullOrEmpty(cmd) == false)
            {
                _data["model"] = model;
                _data["cmd"] = cmd;
            }
            if (String.IsNullOrEmpty(cell) == false)
            {
                _data["cell"] = cell;
            }
            if (search != null && search.Count > 0)
            {
                _data["search"] = search;
            }
        }

        public void StyleForMeta(WebMeta style)
        {
            if (style != null && style.Count > 0)
            {
                _style.Copy(new UIStyle(style));
            }
        }
        UIStyle _style = new UIStyle();
        public UIStyle Style
        {
            get
            {
                return _style;
            }
            set
            {

                _style.Copy(value);

            }
        }
        public void Format(WebMeta format)
        {
            if (format != null && format.Count > 0)
            {
                _data["format"] = format;
            }
        }
        /// <summary>
        ///  提交事件，点击行提交数据并关闭当前页面
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cmd"></param>
        /// <param name="send">如果数据源有此字段，则用此字段，则取此字段值。</param>
        public void Submit(string model, string cmd, string send)
        {
            var click = new UMC.Web.WebMeta().Put("model", model).Put("cmd", cmd);
            if (String.IsNullOrEmpty(send) == false)
            {
                click.Put("send", send); ;
            }

            _data["submit"] = click;

        }
        /// <summary>
        ///  提交事件，点击行提交数据并关闭当前页面
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cmd"></param>
        /// <param name="send">如果数据源有此字段，则用此字段，则取此字段值。</param>
        public void Submit(string model, string cmd, WebMeta send)
        {
            var click = new UMC.Web.WebMeta().Put("model", model).Put("cmd", cmd);
            if (send != null && send.Count > 0)
            {
                click.Put("send", send); ;
            }

            _data["submit"] = click;

        }
        /// <summary>
        ///  点击事件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cmd"></param>
        /// <param name="send">如果数据源有此字段，则用此字段，则取此字段值。</param>
        public void Click(string model, string cmd, string send)
        {
            var click = new UMC.Web.WebMeta().Put("model", model).Put("cmd", cmd);
            if (String.IsNullOrEmpty(send) == false)
            {
                click.Put("send", send); ;
            }

            _data["click"] = click;

        }
        /// <summary>
        ///  点击事件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cmd"></param>
        /// <param name="send">如果send字典值在数据源有此字段，则取此字段值替换。</param>
        public void Click(string model, string cmd, WebMeta send)
        {
            var click = new UMC.Web.WebMeta().Put("model", model).Put("cmd", cmd);
            if (send != null && send.Count > 0)
            {
                click.Put("send", send); ;
            }

            _data["click"] = click;

        }

        void Data.IJSON.Write(System.IO.TextWriter writer)
        {

            _data["style"] = _style;
            UMC.Data.JSON.Serialize(_data, writer);
        }

        void Data.IJSON.Read(string key, object value)
        {

        }
    }
}
