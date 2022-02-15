using System;
using System.Collections;
using System.Xml;
using UMC.Data;
namespace UMC.Data
{
    /// <summary>
    /// 信息配置集合体
    /// </summary>
    public class ProviderConfiguration
    {
        class ProviderHashtable : Hashtable
        {
            ProviderConfiguration pr;
            public override object this[object key]
            {
                get
                {

                    return base[key];
                }
                set
                {
                    if (this.ContainsKey(key))
                    {
                        base[key] = value;
                    }
                    else
                    {
                        this.Add(key, value);
                    }
                }
            }
            public ProviderHashtable(ProviderConfiguration pr)
            {
                this.pr = pr;
            }
            public override void Add(object key, object value)
            {
                base.Add(key, value);
                this.pr._KeyIndex.Add(key);
            }
            public override void Clear()
            {
                base.Clear();
                this.pr._KeyIndex.Clear();
            }
            public override void Remove(object key)
            {
                base.Remove(key);
                this.pr._KeyIndex.Remove(key);
            }
        }
        /// <summary>
        /// 默认初始化
        /// </summary>
        public ProviderConfiguration()
        {
            this._Providers = new ProviderHashtable(this);
        }
        /// <summary>
        /// Provider提供的个数
        /// </summary>
        public int Count
        {
            get
            {
                return this.Providers.Count;
            }
        }



        // Fields
        private string _ProviderType;
        private Hashtable _Providers;
        private ArrayList _KeyIndex = new ArrayList();

        public Provider this[string key]
        {
            get
            {
                return this._Providers[key] as Provider;
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public Provider this[int index]
        {
            get
            {
                return this._Providers[this._KeyIndex[index]] as Provider;
            }
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<String, ProviderConfiguration> _cache = new System.Collections.Concurrent.ConcurrentDictionary<string, ProviderConfiguration>();

        public static System.Collections.Generic.IDictionary<String, ProviderConfiguration> Cache
        {
            get
            {
                return _cache;
            }
        }

        public static ProviderConfiguration GetProvider(string filename)
        {
            if (_cache.ContainsKey(filename))
            {
                return _cache[filename];
            }
            ProviderConfiguration providers = null;

            if (System.IO.File.Exists(filename))
            {
                providers = GetProvider(System.IO.File.OpenRead(filename));
                _cache[filename] = providers;
            }
            return providers;
        }
        /// <summary>
        /// 从xml文件流中得到提供信息
        /// </summary>
        /// <param name="reader">可读流</param>
        public static ProviderConfiguration GetProvider(System.IO.TextReader reader)
        {
            ProviderConfiguration providers = new ProviderConfiguration();
            //providers._ProviderAttribute = filename;
            System.Xml.XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            providers._ProviderType = doc.DocumentElement.Attributes["providerType"] == null ? "" : doc.DocumentElement.Attributes["providerType"].Value;
            foreach (XmlNode node2 in doc.DocumentElement.ChildNodes)
            {
                if (node2.Name == "providers")
                {
                    providers.GetProviders(node2);
                }
            }
            return providers;
        }

        public void WriteTo(System.Xml.XmlDocument doc)
        {
            var provider = doc.CreateElement("providers");
            doc.DocumentElement.AppendChild(provider);
            if (!String.IsNullOrEmpty(this._ProviderType))
            {
                var providerType = doc.CreateAttribute("providerType");
                providerType.Value = this._ProviderType;
                provider.Attributes.Append(providerType);
            }


            for (int i = 0; i < this._KeyIndex.Count; i++)
            {
                Provider pro = (Provider)this._Providers[this._KeyIndex[i]];

                var add = doc.CreateElement("add");
                var name = doc.CreateAttribute("name");
                name.Value = pro.Name;
                add.Attributes.Append(name);
                var type = doc.CreateAttribute("type");
                type.Value = pro.Type;
                add.Attributes.Append(type);

                for (int a = 0; a < pro.Attributes.Count; a++)
                {
                    string str = pro.Attributes[a];
                    if (!String.IsNullOrEmpty(str))
                    {
                        if (str.IndexOf('\n') > -1)
                        {
                            var node = doc.CreateElement(pro.Attributes.GetKey(a));
                            node.AppendChild(doc.CreateCDataSection(str));
                            add.AppendChild(node);
                        }
                        else
                        {
                            var att = doc.CreateAttribute(pro.Attributes.GetKey(a));
                            att.Value = str;
                            add.Attributes.Append(att);
                        }
                    }
                }
                provider.AppendChild(add);

            }

        }
        public void WriteTo(System.IO.Stream outStream)
        {
            var writer = new System.IO.StreamWriter(outStream);
            WriteTo(writer);
        }
        public void WriteTo(string filename)
        {
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
            }
            var file = System.IO.File.Open(filename, System.IO.FileMode.Create);
            try
            {
                WriteTo(file);
            }
            finally
            {
                file.Close();
            }

        }
        public void WriteTo(System.IO.TextWriter writer)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.IndentChars = "\t";
            settings.OmitXmlDeclaration = false;
            WriteTo(System.Xml.XmlWriter.Create(writer, settings));
        }
        public void WriteTo(System.Xml.XmlWriter writer)
        {
            System.Xml.XmlDocument doc = new XmlDocument();
            doc.LoadXml("<umc/>");
            WriteTo(doc);
            doc.Save(writer);
        }
        /// <summary>
        /// 从xml文件流中得到提供信息
        /// </summary>
        /// <param name="stream">可读流</param>
        public static ProviderConfiguration GetProvider(System.IO.Stream stream)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            try
            {
                return GetProvider(reader);
            }
            finally
            {
                stream.Close();
                reader.Close();
            }
        }
        internal static ProviderConfiguration GetProvider(System.Xml.XmlNode node)
        {
            ProviderConfiguration providers = new ProviderConfiguration();
            providers.GetProviders(node);
            return providers;
        }

        void GetProviders(XmlNode node)
        {

            try
            {
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "add":
                            this.Providers[node2.Attributes["name"].Value] = new Provider(node2);
                            break;
                        case "remove":
                            this.Providers.Remove(node2.Attributes["name"].Value);
                            break;
                        case "clear":
                            this.Providers.Clear();
                            break;
                    }
                }
            }
            finally
            {
            }
        }
        public static ProviderConfiguration GetProvider(System.Xml.XPath.XPathNavigator nav)
        {
            ProviderConfiguration providers = new ProviderConfiguration();
            providers.GetProviders(nav);
            return providers;
        }
        void GetProviders(System.Xml.XPath.XPathNavigator nav1)
        {
            var navProviders = nav1.Select("providers");

            while (navProviders.MoveNext())
            {
                var navs = navProviders.Current.SelectChildren(System.Xml.XPath.XPathNodeType.Element);

                while (navs.MoveNext())
                {
                    var cur = navs.Current.Clone();

                    string name = cur.GetAttribute("name", string.Empty);
                    switch (cur.Name.ToLower())
                    {
                        case "add":
                            this.Providers[name] = new Provider(cur);
                            break;
                        case "remove":
                            this.Providers.Remove(name);
                            break;
                        case "clear":
                            this.Providers.Clear();
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 提供者属性，如果是从文件中加裁，则些属性为加裁的文件名
        /// </summary>
        public string ProviderType
        {
            get
            {
                return this._ProviderType;
            }
        }
        /// <summary>
        /// 所有提供者节点信息
        /// </summary>
        public Hashtable Providers
        {
            get
            {
                return this._Providers;
            }
        }
    }


}