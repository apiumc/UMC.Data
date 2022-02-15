using System;
using System.Xml;
using System.Collections.Specialized;
namespace UMC.Data
{
    /// <summary>
    /// 数据配置
    /// </summary>
    public sealed class Provider : IJSON
    {
        // Fields
        private NameValueCollection _ProviderAttributes = new NameValueCollection();
        private string _ProviderName;
        private string _ProviderType;
        private Provider()
        {
        }
        public static Provider Create(string name, string type)
        {
            var provder = new Provider();
            provder._ProviderName = name;
            provder._ProviderType = type;
            return provder;
        }
        public string this[string key]
        {
            get
            {
                return this._ProviderAttributes[key];
            }
        }
        // Methods
        internal Provider(XmlAttributeCollection Attributes)
        {
            this._ProviderName = Attributes["name"].Value;
            this._ProviderType = (Attributes["type"] != null) ? Attributes["type"].Value : String.Empty;
            try
            {
                foreach (XmlAttribute attribute in Attributes)
                {
                    if ((attribute.Name != "name") & (attribute.Name != "type"))
                    {
                        this._ProviderAttributes.Add(attribute.Name, attribute.Value);
                    }
                }
            }
            finally
            {
            }
        }
        internal Provider(System.Xml.XPath.XPathNavigator nav)
        {
            this._ProviderName = nav.GetAttribute("name", String.Empty);//.Value;
            this._ProviderType = nav.GetAttribute("type", String.Empty);
            var ns = nav.SelectChildren(System.Xml.XPath.XPathNodeType.Element);
            if (nav.MoveToFirstAttribute())
            {
                if ((nav.Name != "name") & (nav.Name != "type"))
                {
                    this._ProviderAttributes.Add(nav.Name, nav.Value);
                }
                while (nav.MoveToNextAttribute())
                {
                    if ((nav.Name != "name") & (nav.Name != "type"))
                    {
                        this._ProviderAttributes.Add(nav.Name, nav.Value);
                    }
                }

            }
            while (ns.MoveNext())
            {
                this._ProviderAttributes.Add(ns.Current.Name, ns.Current.Value);
            }
        }

        internal Provider(XmlNode node)
            : this(node.Attributes)
        {
            foreach (XmlNode cnode in node.ChildNodes)
            {
                if (cnode.NodeType == XmlNodeType.CDATA || cnode.NodeType == XmlNodeType.Text)
                {
                    this._ProviderAttributes.Add("nodeValue", cnode.InnerText);
                }
                else if (cnode.NodeType == XmlNodeType.Element)
                {
                    this._ProviderAttributes.Add(cnode.Name, cnode.InnerText);
                }
            }
        }

        /// <summary>
        /// 属性
        /// </summary>
        public NameValueCollection Attributes
        {
            get
            {
                return this._ProviderAttributes;
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return this._ProviderName;
            }
        }
        /// <summary>
        /// 提交的类型
        /// </summary>
        public string Type
        {
            get
            {
                return this._ProviderType;
            }
        }

        #region IJSONConvert Members

        void IJSON.Write(System.IO.TextWriter writer)
        {
            writer.Write('{');
            writer.Write("\"name\":");
            JSON.Serialize(this._ProviderName, writer);
            writer.Write(',');
            writer.Write("\"type\":");
            JSON.Serialize(this._ProviderType, writer);
            for (var i = 0; i < this._ProviderAttributes.Count; i++)
            {
                var name = this._ProviderAttributes.GetKey(i);
                if (!String.IsNullOrEmpty(name) && name != "name" && name != "type")
                {
                    writer.Write(',');

                    JSON.Serialize(name, writer);
                    writer.Write(':');
                    JSON.Serialize(this._ProviderAttributes.Get(i), writer);
                }
            }
            writer.Write('}');
        }

        void IJSON.Read(string key, object value)
        {
            switch (key)
            {
                case "name":
                    this._ProviderName = (value ?? String.Empty).ToString();
                    break;
                case "type":
                    this._ProviderType = (value ?? String.Empty).ToString();
                    break;
                default:
                    this._ProviderAttributes.Add(key, (value ?? String.Empty).ToString());
                    break;
            }
        }

        #endregion
    }



}