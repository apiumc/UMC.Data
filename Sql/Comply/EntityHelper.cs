using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{

    class EntityHelper
    {
        public List<object> Arguments
        {
            get;
            private set;
        }
        public class FieldInfo
        {
            public System.Reflection.PropertyInfo Property
            {
                get;
                set;
            }
            public FieldAttribute Attribute
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }
            public int FieldIndex
            {
                get;
                set;
            }
        }

        /// <summary>
        /// 类型的表属性
        /// </summary>
        public TableAttribute TableInfo
        {
            get;
            set;
        }
        /// <summary>
        /// 类型的所有属性
        /// </summary>
        public List<FieldInfo> Fields
        {
            get;
            set;
        }
        public Type ObjType;
        DbProvider Provider;
        /// <summary>
        /// 对应的表字段
        /// </summary>
        public EntityHelper(DbProvider dbProvider, Type type, String tableName)
        {
            this.Provider = dbProvider;
            this.Arguments = new List<object>();
            this.Fields = new List<FieldInfo>();
            this.ObjType = type;
            if (String.IsNullOrEmpty(tableName))
            {
                object[] objs = type.GetCustomAttributes(false);
                foreach (object o in objs)
                {
                    if (o is TableAttribute)
                    {
                        TableInfo = o as TableAttribute;
                        tableName = TableInfo.Name;
                        break;
                    }
                }
                if (String.IsNullOrEmpty(tableName))
                {
                    tableName = type.Name;
                }
            }

            Bind(type, tableName);
        }

        public EntityHelper(DbProvider dbProvider, Type type)
            : this(dbProvider, type, String.Empty)
        {
        }

        String ObjectName(String name)
        {
            if (this.Provider.Builder != null)
            {
                return this.Provider.Builder.Column(name);
            }

            return name;
        }
        void Bind(Type type, string tableName)
        {
            this.TableInfo = new TableAttribute();

            TableInfo.Name = ObjectName(tableName);
            var PropertyInfos = type.GetProperties();
            foreach (System.Reflection.PropertyInfo prInfo in PropertyInfos)
            {
                if (prInfo.CanRead)
                {
                    var objs = prInfo.GetCustomAttributes(false);
                    FieldAttribute field = new FieldAttribute();
                    foreach (object o in objs)
                    {
                        if (o is FieldAttribute)
                        {
                            field = o as FieldAttribute;
                            if (String.IsNullOrEmpty(field.Field))
                            {
                                field.Field = prInfo.Name;
                            }
                            break;
                        }
                    }
                    if (String.IsNullOrEmpty(field.Field))
                    {
                        field.Field = prInfo.Name;
                    }

                    var enField = new FieldInfo();
                    enField.Property = prInfo;
                    enField.Attribute = field;
                    enField.Name = prInfo.Name;
                    Fields.Add(enField);
                }
            }
        }
        /// <summary>
        /// 创建插入实体SQL脚本
        /// </summary>
        /// <returns></returns>
        public string CreateInsertText(object entity)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0}(", TableInfo.Name);
            StringBuilder sb2 = new StringBuilder();
            sb2.Append("VALUES(");
            bool IsMush = false;
            this.Arguments.Clear();

            for (int i = 0; i < Fields.Count; i++)
            {
                var field = Fields[i];
                if (field.Attribute.Insert)
                {
                    if (field.Property == null)
                    {
                        continue;
                    }
                    var value = field.Property.GetValue(entity, null) ?? null;
                    if (value == null)
                    {
                        continue;
                    }

                    if (IsMush)
                    {
                        sb.Append(",");
                        sb2.Append(",");
                    }
                    else
                    {
                        IsMush = true;
                    }

                    sb.Append(Provider.QuotePrefix);
                    sb.Append(ObjectName(field.Attribute.Field));
                    sb.Append(Provider.QuoteSuffix);

                    sb2.Append('{');
                    sb2.Append(this.Arguments.Count);
                    sb2.Append('}');

                    this.Arguments.Add(this.GetArgumentValue(value, field.Property));
                }
            }

            sb.Append(")");
            sb2.Append(")");
            return sb.ToString() + sb2.ToString();

        }
        /// <summary>
        /// 创建实体ＳＱＬ删除脚本
        /// </summary>
        /// <returns></returns>
        public string CreateDeleteText()
        {
            return String.Format("DELETE FROM {0} ", TableInfo.Name);

        }
        /// <summary>
        /// 创建实体查询SQL脚本
        /// </summary>
        /// <returns></returns>
        public string CreateSelectText(object entity)
        {
            this.IsClearIndex = false;
            bool IsEntity = false;
            if (entity != null)
            {
                IsEntity = this.ObjType.Equals(entity.GetType());
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            bool IsMush = false; int index = 0;


            foreach (var field in Fields)
            {
                if (field.Attribute.Select)
                {
                    if (IsEntity)
                    {
                        if (field.Property != null)
                        {
                            var value = field.Property.GetValue(entity, null) ?? null;
                            if (value == null)
                            {
                                continue;
                            }
                        }
                    }
                    if (IsMush)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        IsMush = true;
                    }
                    String sfield = field.Attribute.Field;
                    if (field.Attribute.IsExpression)
                    {
                        if (sfield.IndexOf(" as ", StringComparison.CurrentCultureIgnoreCase) > -1)
                        {
                            sb.Append(sfield);
                        }
                        else
                        {
                            sb.Append(sfield);
                            sb.Append(" as ");
                            if (field.Property != null)
                            {
                                sb.Append(Provider.QuotePrefix);
                                sb.Append(ObjectName(field.Property.Name));
                                sb.Append(Provider.QuoteSuffix);
                            }
                            else
                            {
                                sb.Append(field.Name);
                            }
                        }
                    }
                    else
                    {
                        index = sfield.IndexOf(' ');
                        if (index == -1) index = sfield.IndexOf('.');
                        if (index == -1)
                        {
                            sb.Append(Provider.QuotePrefix);
                            sb.Append(ObjectName(sfield));
                            sb.Append(Provider.QuoteSuffix);
                        }
                        else
                        {
                            sb.Append(sfield);
                        }
                    }
                }
            }

            sb.AppendFormat(" FROM {0} ", TableInfo.Name);

            return sb.ToString();
        }
        public bool IsClearIndex
        {
            get;
            set;

        }
        void SetIndex(System.Data.IDataReader dr)
        {
            var list = new List<string>();
            for (var i = 0; i < dr.FieldCount; i++)
            {
                list.Add(dr.GetName(i).ToLower());
            }
            for (int i = 0; i < Fields.Count; i++)
            {
                Fields[i].FieldIndex = list.FindIndex(s => Fields[i].Attribute.Field.ToLower() == s);
            }
            IsClearIndex = true;
        }
        /// <summary>
        /// 创建实体更新脚本
        /// </summary>
        /// <returns></returns>
        string GetUpdateText(object entity, string format)
        {
            if (String.IsNullOrEmpty(format))
            {
                format = "{1}";
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET  ", TableInfo.Name);

            bool IsMush = false;

            for (int i = 0; i < Fields.Count; i++)
            {
                var fd = Fields[i];
                if (fd.Attribute.Update)
                {
                    if (fd.Property == null)
                    {
                        continue;
                    }

                    var value = fd.Property.GetValue(entity, null) ?? null;
                    if (value == null)
                    {
                        continue;
                    }
                    if (IsMush)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        IsMush = true;
                    }
                    var Field = Provider.QuotePrefix + ObjectName(Fields[i].Attribute.Field) + Provider.QuoteSuffix;
                    var Value = "{" + this.Arguments.Count + "}";
                    sb.Append(Field);
                    sb.Append('=');

                    sb.AppendFormat(format, Field, Value);


                    this.Arguments.Add(this.GetArgumentValue(value, Fields[i].Property));


                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 创建实体更新脚本
        /// </summary>
        /// <returns></returns>
        public string AppendUpdateText(StringBuilder sb, object entity, string format)
        {
            if (String.IsNullOrEmpty(format))
            {
                format = "{1}";
            }

            for (int i = 0; i < Fields.Count; i++)
            {
                var fd = Fields[i];
                if (fd.Attribute.Update)
                {
                    if (fd.Property == null)
                    {
                        continue;
                    }

                    var value = fd.Property.GetValue(entity, null) ?? null;
                    if (value == null)
                    {
                        continue;
                    }

                    sb.Append(",");

                    var Field = Provider.QuotePrefix + ObjectName(Fields[i].Attribute.Field) + Provider.QuoteSuffix;
                    var Value = "{" + this.Arguments.Count + "}";
                    sb.Append(Field);
                    sb.Append('=');

                    sb.AppendFormat(format, Field, Value);


                    this.Arguments.Add(this.GetArgumentValue(value, Fields[i].Property));


                }
            }
            return sb.ToString();
        }



        public string CreateUpdateText(string format, System.Collections.IDictionary fieldValues)
        {
            if (String.IsNullOrEmpty(format))
            {
                format = "{1}";
            }

            this.Arguments.Clear();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET  ", TableInfo.Name);

            var f = fieldValues.GetEnumerator();
            bool IsMush = false;
            while (f.MoveNext())
            {
                if (IsMush)
                {
                    sb.Append(",");
                }
                else
                {
                    IsMush = true;
                }
                var Field = Provider.QuotePrefix + ObjectName(f.Key as string) + Provider.QuoteSuffix;
                var Value = "{" + this.Arguments.Count + "}";

                sb.Append(Field);
                sb.Append('=');

                sb.AppendFormat(format, Field, Value);

                this.Arguments.Add(f.Value);
            }

            return sb.ToString();
        }
        object GetArgumentValue(object value, System.Reflection.PropertyInfo protype)
        {
            if (protype.GetCustomAttributes(typeof(JSONAttribute), true).Length > 0
                ||
                protype.PropertyType.GetCustomAttributes(typeof(JSONAttribute), true).Length > 0)
            {
                return JSON.Serialize(value, true);
            }
            else
            {
                return value;
            }
        }
        public string CreateUpdateText(string format, object entity, params string[] proNames)
        {
            if (String.IsNullOrEmpty(format))
            {
                format = "{1}";
            }
            this.Arguments.Clear();
            if (proNames.Length == 0)
            {
                return GetUpdateText(entity, format);
            }
            List<String> strs = new List<string>(proNames);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" UPDATE {0} SET  ", TableInfo.Name);

            bool IsMush = false;

            for (int i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Attribute.Update && strs.Exists(str => str.ToLower() == Fields[i].Property.Name.ToLower()))
                {
                    if (IsMush)
                    {
                        sb.Append(",");
                    }

                    else
                    {
                        IsMush = true;
                    }
                    var Field = Provider.QuotePrefix + ObjectName(Fields[i].Attribute.Field) + Provider.QuoteSuffix;
                    var Value = "{" + this.Arguments.Count + "}";

                    sb.Append(Field);
                    sb.Append('=');

                    sb.AppendFormat(format, Field, Value);

                    this.Arguments.Add(GetArgumentValue(entity, Fields[i].Property) ?? DBNull.Value);
                }
            }
            return sb.ToString();
        }
        public static void SetValue(object obvalue, System.Reflection.PropertyInfo objPropertyInfo, object drObj)
        {
            if (drObj != null)
            {

                if (drObj == DBNull.Value)
                {

                }
                else
                {
                    Type proType = objPropertyInfo.PropertyType;
                    if (proType.IsEnum)
                    {
                        objPropertyInfo.SetValue(obvalue, Enum.ToObject(proType, drObj), null);
                    }
                    else if (proType.IsGenericType)
                    {
                        if (CBO.Nullable == proType.GetGenericTypeDefinition())
                        {
                            var ndType = proType.GetGenericArguments()[0];
                            if (ndType.IsEnum)
                            { 
                                if (drObj is string)
                                {
                                    objPropertyInfo.SetValue(obvalue, Enum.Parse(ndType, drObj as string), null);
                                }
                                else
                                {
                                    objPropertyInfo.SetValue(obvalue, Enum.ToObject(ndType, drObj), null);
                                }
                            }
                            else if (drObj is string)
                            {
                                objPropertyInfo.SetValue(obvalue, UMC.Data.Reflection.Parse(drObj as string, ndType), null);
                            }
                            else if (ndType == typeof(DateTime))
                            {
                                if (drObj is DateTime)
                                {

                                    objPropertyInfo.SetValue(obvalue, drObj, null);
                                }
                                else
                                {
                                    objPropertyInfo.SetValue(obvalue, UMC.Data.Utility.TimeSpan(Convert.ToInt32(drObj)), null);
                                }
                            }
                            else
                            {
                                objPropertyInfo.SetValue(obvalue, Convert.ChangeType(drObj, ndType), null);
                            }
                        }

                    }
                    else
                    {
                        if (drObj is string)
                        {
                            if (objPropertyInfo.GetCustomAttributes(typeof(JSONAttribute), true).Length > 0
                                ||
                                objPropertyInfo.PropertyType.GetCustomAttributes(typeof(JSONAttribute), true).Length > 0)
                            {
                                var str = (string)drObj;
                                if (String.IsNullOrEmpty(str) == false)
                                {
                                    objPropertyInfo.SetValue(obvalue, JSON.Deserialize(str, proType), null);
                                }
                            }
                            else
                            {
                                objPropertyInfo.SetValue(obvalue, Convert.ChangeType(drObj, proType), null);
                            }
                        }
                        else if (drObj is DateTime && proType == typeof(String))
                        {
                            var date = (DateTime)drObj;
                            if (date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                            {

                                objPropertyInfo.SetValue(obvalue, date.ToString("yyyy-MM-dd"), null);
                            }
                            else
                            {
                                objPropertyInfo.SetValue(obvalue, date.ToString("yyyy-MM-dd HH:mm:ss"), null);

                            }
                        }
                        else
                        {
                            objPropertyInfo.SetValue(obvalue, Convert.ChangeType(drObj, proType), null);
                        }
                    }
                }
            }
        }

        public Object CreateObject(object obvalue, System.Data.IDataReader dr)
        {
            if (IsClearIndex == false)
            {
                SetIndex(dr);
            }

            for (int i = 0; i < this.Fields.Count; i++)
            {
                var proField = this.Fields[i];
                var objPropertyInfo = proField.Property;

                if (proField.Attribute.Select)
                {

                    var fIndex = proField.FieldIndex;
                    if (fIndex == -1)
                    {
                        continue;
                    }

                    if (objPropertyInfo.CanWrite)
                    {
                        SetValue(obvalue, objPropertyInfo, dr[fIndex]);
                    }
                }
            }
            return obvalue;
        }

    }
}
