using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Data;

namespace UMC.Data
{
    /// <summary>
    /// JSON解析工具
    /// </summary>
    public sealed class JSON
    {

        class JSONTextWriter : System.IO.TextWriter
        {
            public JSONTextWriter(System.IO.TextWriter writer)
            {
                this._out = writer;
            }
            System.IO.TextWriter _out;
            public override Encoding Encoding
            {
                get { return this._out.Encoding; }
            }
            public override void Write(char value)
            {
                switch (value)
                {
                    case '\'':
                        this._out.Write("\\'");
                        break;
                    case '\\':
                        this._out.Write("\\\\");
                        break;
                    case '\b':
                        this._out.Write("\\b");
                        break;
                    case '\f':
                        this._out.Write("\\f");
                        break;
                    case '\n':
                        this._out.Write("\\n");
                        break;
                    case '\t':
                        this._out.Write("\\t");
                        break;
                    case '\r':
                        this._out.Write("\\r");
                        break;
                    default:
                        this._out.Write(value);
                        break;
                }
            }
            public override void Close()
            {
                this._out.Close();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this._out.Dispose();
                }
            }

            public override void Flush()
            {
                this._out.Flush();
            }


            public override string NewLine
            {
                get
                {
                    return this._out.NewLine;
                }
                set
                {
                    this._out.NewLine = value;
                }

            }
        }
        public static System.IO.TextWriter JSONWriter(System.IO.TextWriter writer)
        {
            if (writer is JSONTextWriter)
            {
                return writer;
            }
            return new JSONTextWriter(writer);
        }
        string _DateFormat;
        /// <summary>
        /// JSON标识强类型的Key
        /// </summary>
        const string Constructor = ":constructor";
        bool IsSerializer;
        #region IJSONConvert Members

        class JSONer : IJSON
        {
            public string expression;
            #region IJSONConvert Members

            void IJSON.Write(System.IO.TextWriter writer)
            {
                writer.Write(expression);
            }

            void IJSON.Read(string key, object value)
            {
            }

            #endregion
        }


        #endregion
        /// <summary>
        /// 创建JS表示式
        /// </summary>
        public static Object Expression(String expression)
        {
            var json = new JSONer();
            json.expression = expression;
            return json;
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        /// <param name="json">JSON</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            if (String.IsNullOrEmpty(json))
            {
                return default(T);
            }
            int i = -1;
            return (T)Deserialize(ref json, ref i, typeof(T));
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        /// <param name="json">json</param>
        /// <param name="type">转化的类型</param>
        /// <returns></returns>
        public static object Deserialize(string json, Type type)
        {
            if (String.IsNullOrEmpty(json))
            {
                return null;
            }
            int i = -1;
            return Deserialize(ref json, ref i, type);
        }
        /// <summary>
        /// JSON字典化
        /// </summary>
        /// <param name="json">JSON</param>
        /// <returns></returns>
        public static object Deserialize(string json)
        {
            if (String.IsNullOrEmpty(json))
            {
                return null;
            }
            int i = -1;
            return Deserialize(ref json, ref i);
        }
        static object Deserialize(ref string input, ref int index, Type type)
        {
            var isArray = false;
            var isObject = false;
            IList list = null;

            object objValue = null;
            for (index++; index < input.Length; index++)
            {

                switch (input[index])
                {
                    case '[':
                        isArray = true;
                        if (type.IsGenericType)
                        {
                            objValue = Reflection.CreateInstance(type);
                            list = objValue as IList;

                            RemoveEmpty(ref input, ref index);
                            if (input[index + 1] == ']')
                            {
                                index++;
                                return objValue;
                            }
                            var gType = type.GetGenericArguments()[0];

                            list.Add(Deserialize(ref input, ref index, gType));

                        }
                        else if (type.IsArray)
                        {

                            objValue = list = new ArrayList();
                            var gType = type.GetElementType();
                            RemoveEmpty(ref input, ref index);
                            if (input[index + 1] == ']')
                            {
                                index++;
                                return Array.CreateInstance(gType, 0);
                            }
                            list.Add(Deserialize(ref input, ref index, gType));

                        }
                        else
                        {
                            throw new System.ArrayTypeMismatchException(type.ToString());
                        }
                        break;
                    case '{':
                        if (type == typeof(Hashtable))
                        {
                            index--;
                            return Deserialize(ref input, ref index);
                        }
                        else if (type.IsArray)
                        {
                            var eType = type.GetElementType();
                            var arr = Array.CreateInstance(eType, 1);
                            index--;
                            arr.SetValue(Deserialize(ref input, ref index, eType), 0);
                            return arr;
                        }
                        isObject = true;
                        RemoveEmpty(ref input, ref index);
                        if (input[index + 1] == '}')
                        {
                            index++;
                            return Reflection.CreateInstance(type);
                        }
                        var key = Deserialize(ref input, ref index).ToString();
                        if (key == Constructor)
                        {
                            var sType = Deserialize(ref input, ref index).ToString();
                            type = UMC.Data.Reflection.CreateType(sType);

                            objValue = Reflection.CreateInstance(type);
                            break;
                        }
                        objValue = Reflection.CreateInstance(type);
                        if (objValue is IJSON)
                        {
                            if (objValue is IJSONType)
                            {
                                ((IJSON)objValue).Read(key, Deserialize(ref input, ref index, ((IJSONType)objValue).GetType(key)));
                            }
                            else
                            {
                                ((IJSON)objValue).Read(key, Deserialize(ref input, ref index));
                            }
                        }
                        else
                        {
                            var prototyType = type.GetProperty(key
                                , BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (prototyType != null)
                            {
                                prototyType.SetValue(objValue, Deserialize(ref input, ref index, prototyType.PropertyType), null);
                            }
                            else
                            {
                                Deserialize(ref input, ref index);
                            }
                        }
                        break;
                    case ',':
                        if (isArray)
                        {
                            if (type.IsArray)
                            {
                                list.Add(Deserialize(ref input, ref index, type.GetElementType()));
                            }
                            else if (type.IsGenericType)
                            {
                                list.Add(Deserialize(ref input, ref index, type.GetGenericArguments()[0]));
                            }
                            else
                            {
                                Deserialize(ref input, ref index);
                            }
                        }
                        else if (isObject)
                        {
                            var key2 = Deserialize(ref input, ref index) as string;
                            if (objValue is IJSON)
                            {
                                if (objValue is IJSONType)
                                {
                                    ((IJSON)objValue).Read(key2, Deserialize(ref input, ref index, ((IJSONType)objValue).GetType(key2)));
                                }
                                else
                                {
                                    ((IJSON)objValue).Read(key2, Deserialize(ref input, ref index));
                                }
                            }
                            else
                            {
                                var prototyType2 = type.GetProperty(key2.ToString(), BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Instance);

                                if (prototyType2 != null)
                                {
                                    var v = Deserialize(ref input, ref index, prototyType2.PropertyType);
                                    if (prototyType2.CanWrite)
                                    {
                                        prototyType2.SetValue(objValue, v, null);
                                    }

                                }
                                else
                                {
                                    Deserialize(ref input, ref index);
                                }
                            }
                        }
                        break;
                    case ']':
                        if (isArray)
                        {
                            if (type.IsArray)
                            {
                                return ((ArrayList)list).ToArray(type.GetElementType());
                            }
                            return objValue;
                        }
                        else
                        {
                            throw new System.FormatException("非JSON格式");
                        }
                    case '}':
                        if (isObject)
                        {
                            return objValue;
                        }
                        else
                        {
                            throw new System.FormatException("非JSON格式");
                        }
                    case ':':
                        break;
                    case '"':
                    case '\'':
                        return Reflection.Parse(Deserialize(ref input, ref index, input[index]), type);

                    case '$':
                    case '_':
                    case '.':
                    case '-':
                        return Data.Reflection.Parse(Deserialize2(ref input, ref index), type);
                    default:
                        var code = input[index];
                        if ((code > 64 && code < 91) || (code > 96 && code < 123) || (code > 47 && code < 58))
                        {
                            var c = Deserialize2(ref input, ref index);
                            switch (c)
                            {
                                case "undefined":
                                case "null":
                                    return null;
                                default:

                                    return Reflection.Parse(c, type);
                            }
                        }
                        break;
                }

            }
            return objValue;

        }
        static void RemoveEmpty(ref string input, ref int index)
        {
            var isb = false;
            for (index++; index < input.Length; index++)
            {
                switch (input[index])
                {
                    case '\t':
                    case ' ':
                    case '\b':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        isb = true;
                        break;
                }
                if (isb)
                {
                    index--;
                    break;
                }
            }


        }
        /// <summary>
        /// 反序列化
        /// </summary>
        public static object Deserialize(string input, ref int startIndex, Type type)
        {
            startIndex--;
            return Deserialize(ref input, ref startIndex, type);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string input, ref int startIndex)
        {
            return (T)Deserialize(ref input, ref startIndex, typeof(T));
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static object Deserialize(string input, ref int startIndex)
        {

            startIndex--;
            return Deserialize(ref input, ref startIndex);
        }
        static object Deserialize(ref string input, ref int index)
        {
            var isArray = false;
            var isObject = false;
            ArrayList list = null;
            Hashtable hask = null;
            for (index++; index < input.Length; index++)
            {

                switch (input[index])
                {
                    case '[':
                        isArray = true;
                        list = new ArrayList();
                        RemoveEmpty(ref input, ref index);
                        if (input[index + 1] == ']')
                        {
                            index++;
                            return list.ToArray();
                        }
                        list.Add(Deserialize(ref input, ref index));

                        break;
                    case '{':
                        isObject = true;
                        var beginIndex = index;
                        RemoveEmpty(ref input, ref index);
                        if (input[index + 1] == '}')
                        {
                            index++;
                            return hask ?? new Hashtable();
                        }
                        var key = Deserialize(ref input, ref index).ToString();
                        if (key == Constructor)
                        {
                            var type = Reflection.CreateType(Deserialize(ref input, ref index).ToString());
                            index = beginIndex - 1;
                            return Deserialize(ref input, ref index, type);
                        }
                        else
                        {
                            hask = new Hashtable();
                            hask[key] = Deserialize(ref input, ref index);
                        }
                        break;
                    case ',':
                        if (isArray)
                        {
                            list.Add(Deserialize(ref input, ref index));
                        }
                        else if (isObject)
                        {
                            var key2 = Deserialize(ref input, ref index);
                            hask[key2] = Deserialize(ref input, ref index);
                        }
                        break;
                    case ']':
                        if (isArray)
                        {
                            return list.ToArray();
                        }
                        else
                        {
                            throw new System.FormatException("非JSON格式");
                        }
                    case '}':
                        if (isObject)
                        {
                            return hask;
                        }
                        else
                        {
                            throw new System.FormatException("非JSON格式");
                        }
                    case ':':
                        break;
                    case '"':
                    case '\'':
                        return Deserialize(ref input, ref index, input[index]);

                    case '$':
                    case '_':
                    case '.':
                        return Deserialize2(ref input, ref index);
                    default:
                        var code = input[index];

                        if (code == 45 || (code > 64 && code < 91) || (code > 96 && code < 123) || (code > 47 && code < 58))
                        {
                            var c = Deserialize2(ref input, ref index);
                            switch (c)
                            {
                                case "undefined":
                                case "null":
                                    return null;
                                default:
                                    return c;
                            }
                        }
                        break;
                }

            }
            throw new System.FormatException("非JSON格式");
        }
        static string Deserialize2(ref string input, ref int index)
        {
            var b = index;
            var sb = new StringBuilder();
            sb.Append(input[b]);
            for (var i = index + 1; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case ']':
                    case '}':
                    case ':':
                    case ',':
                        index = i - 1;
                        return sb.ToString().Trim();
                    default:
                        sb.Append(input[i]);
                        break;

                }
            }
            index = input.Length - 1;
            return sb.ToString().Trim();

        }
        static string Deserialize(ref string input, ref int index, char cher)
        {
            index++;

            var isTo = false;
            var sb = new StringBuilder();

            for (; index < input.Length; index++)
            {
                var ichar = input[index];
                if (ichar == '\\')
                {
                    if (isTo)
                    {
                        sb.Append('\\');
                        isTo = false;
                    }
                    else
                    {
                        isTo = true;
                    }

                }
                else if (isTo)
                {
                    isTo = false;
                    switch (ichar)
                    {
                        case 'u':
                            byte[] codes = new byte[2];
                            codes[1] = Convert.ToByte(input.Substring(index + 1, 2), 16);
                            codes[0] = Convert.ToByte(input.Substring(index + 3, 2), 16);
                            sb.Append(Encoding.Unicode.GetString(codes));
                            index += 4;
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        default:
                            sb.Append(ichar);
                            break;
                    }
                }
                else if (ichar == cher)
                {
                    break;

                }
                else
                {
                    sb.Append(ichar);
                    isTo = false;
                }
            }
            if (index != input.Length)
            {
                return sb.ToString();
            }
            throw new System.FormatException("非JSON格式");

        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="obj">序列化对象</param>
        public static void Serialize(object obj, System.Text.StringBuilder sb)
        {
            Serialize(obj, sb, false);
        }
        public static void Serialize(object obj, System.Text.StringBuilder sb, bool serializer)
        {
            System.IO.StringWriter sInsert = new System.IO.StringWriter(sb);
            Serialize(obj, sInsert, serializer);
        }
        public static void Serialize(object obj, System.IO.TextWriter writer)
        {
            Serialize(obj, writer, false);
        }
        public static string Serialize(object obj, string dateFormat)
        {
            var writer = new System.IO.StringWriter();
            Serialize(obj, writer, dateFormat);
            return writer.ToString();
        }
        public static void Serialize(object obj, System.IO.TextWriter writer, string dateFormat)
        {
            var json = new JSON();
            json.IsSerializer = false;
            json._DateFormat = dateFormat;
            json.SerializeObject(obj, writer);
        }
        public static void Serialize(object obj, System.IO.TextWriter writer, bool serializer)
        {
            var json = new JSON();
            json.IsSerializer = serializer;
            json.SerializeObject(obj, writer);
        }

        public static string Serialize(object obj, bool serializer)
        {
            var writer = new System.IO.StringWriter();
            Serialize(obj, writer, serializer);
            return writer.ToString();

        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return Serialize(obj, false);
        }
        void Serialize(System.Data.DataRow dr, System.IO.TextWriter writer)
        {
            writer.Write('{');
            var b = false;
            for (int m = 0; m < dr.Table.Columns.Count; m++)
            {
                var v = dr[m];
                if (v != null)
                {
                    if (b)
                    {
                        writer.Write(',');
                    }
                    b = true;
                    this.SerializeObject(dr.Table.Columns[m].ColumnName, writer);
                    writer.Write(':');
                    this.SerializeObject(v, writer);
                }
            }
            writer.Write('}');
        }
        void Serialize(IDictionaryEnumerator em, System.IO.TextWriter writer)
        {
            writer.Write('{');
            bool bo = false;
            while (em.MoveNext())
            {
                if (bo)
                {
                    writer.Write(',');
                }
                else
                {
                    bo = true;
                }
                this.SerializeObject(em.Key, writer);
                writer.Write(':');
                this.SerializeObject(em.Value, writer);

            }
            writer.Write('}');
        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="dic">序列化字典对象</param>
        void Serialize(System.Collections.IDictionary dic, System.IO.TextWriter writer)
        {
            IDictionaryEnumerator em = dic.GetEnumerator();
            Serialize(em, writer);
        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="table">数据表对象</param>
        void Serialize(DataTable table, System.IO.TextWriter writer)
        {
            writer.Write('[');
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (i != 0)
                {
                    writer.Write(',');
                }
                writer.Write('{');
                bool b = false;
                for (int m = 0; m < table.Columns.Count; m++)
                {
                    var v = table.Rows[i][m];
                    if (v != null)
                    {
                        if (b)
                        {
                            writer.Write(',');
                        }
                        b = true;
                        writer.Write("\"{0}\":", table.Columns[m].ColumnName);
                        this.SerializeObject(v, writer);
                    }
                }
                writer.Write('}');
            }
            writer.Write(']');
        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="obj">序列化对象</param>
        void SerializeObject(object obj, System.IO.TextWriter writer)
        {
            if (obj != null)
            {

                Type type = obj.GetType();
                switch (type.FullName)
                {
                    case "System.SByte":
                    case "System.Byte":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Single":
                        writer.Write(obj.ToString());
                        return;
                    case "System.Double":
                        writer.Write('"');
                        writer.Write(obj);
                        writer.Write('"');
                        return;
                    case "System.Decimal":
                        writer.Write('"');
                        writer.Write("{0:0.00}", obj);
                        writer.Write('"');
                        return;
                }
                if (obj is IJSON)
                {
                    //writer.Write('{');
                    ((IJSON)obj).Write(writer);
                    //writer.Write('}');
                }
                else if (obj == System.DBNull.Value)
                {
                    writer.Write("\"\"");
                }
                else if (type.Equals(typeof(System.Boolean)))
                {
                    writer.Write(obj.ToString().ToLower());
                }
                else if (type.IsEnum)
                {
                    writer.Write('"');
                    writer.Write(obj.ToString());
                    writer.Write('"');
                }
                else if (type.Equals(typeof(System.String)))
                {
                    writer.Write('"');
                    string strs = (String)obj;
                    for (int i = 0; i < strs.Length; i++)
                    {

                        char c = strs[i];
                        switch (c)
                        {
                            case '"':
                                writer.Write("\\\"");
                                break;
                            case '\\':
                                writer.Write("\\\\");
                                break;
                            case '\b':
                                writer.Write("\\b");
                                break;
                            case '\f':
                                writer.Write("\\f");
                                break;
                            case '\n':
                                writer.Write("\\n");
                                break;
                            case '\t':
                                writer.Write("\\t");
                                break;
                            case '\r':
                                writer.Write("\\r");
                                break;
                            default:
                                writer.Write(c);
                                break;
                        }
                    }
                    writer.Write('"');
                }
                else if (type.Equals(typeof(byte[])))
                {
                    writer.Write('"');
                    writer.Write(Convert.ToBase64String(obj as byte[]));
                    writer.Write('"');

                }
                else if (type.Equals(typeof(DateTime)))
                {
                    DateTime date = (DateTime)obj;
                    if (this.IsSerializer)
                    {
                        writer.Write(Reflection.TimeSpan(date));
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(_DateFormat) == false)
                        {
                            switch (_DateFormat)
                            {
                                case "ts":
                                    writer.Write(Reflection.TimeSpan(date));
                                    break;
                                case "Date":
                                    writer.Write(" new Date({0},{1},{2},{3:H,m,s})", date.Year, date.Month - 1, date.Day, date);
                                    break;
                                default:
                                    this.SerializeObject(date.ToString(_DateFormat), writer);
                                    break;
                            }

                        }
                        else if (date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                        {
                            writer.Write('"');
                            writer.Write(date.ToString("yyyy-MM-dd"));
                            writer.Write('"');
                        }
                        else
                        {
                            writer.Write('"');
                            writer.Write(date.ToString("yyyy.M.d HH:mm"));
                            writer.Write('"');
                        }
                    }
                }
                else if (type.IsPrimitive)
                {
                    writer.Write('"');
                    writer.Write(obj.ToString());
                    writer.Write('"');
                }
                else if (type.IsValueType)
                {
                    writer.Write('"');
                    writer.Write(obj.ToString());
                    writer.Write('"');
                }
                else if (type.IsArray)
                {
                    Array array = obj as Array;

                    writer.Write('[');

                    for (int i = 0; i < array.Length; i++)
                    {
                        this.SerializeObject(array.GetValue(i), writer);
                        if (i != array.Length - 1)
                        {
                            writer.Write(',');
                        }
                    }
                    writer.Write(']');
                }
                else if (obj is System.Collections.IDictionary)
                {
                    Serialize((System.Collections.IDictionary)obj, writer);
                }
                else if (obj is IDictionaryEnumerator)
                {
                    var dicEnum = obj as IDictionaryEnumerator;
                    dicEnum.Reset();
                    this.Serialize(dicEnum, writer);
                }
                else if (obj is System.Uri)
                {
                    this.SerializeObject(((System.Uri)obj).AbsoluteUri, writer);
                }
                else if (obj is System.Data.DataTable)
                {
                    this.Serialize((System.Data.DataTable)obj, writer);
                }
                else if (obj is System.Data.DataRow)
                {
                    this.Serialize((System.Data.DataRow)obj, writer);
                }
                else if (obj is System.Collections.IEnumerable)
                {
                    writer.Write('[');
                    var IsNext = false;
                    foreach (Object objea in (System.Collections.IEnumerable)obj)
                    {
                        if (IsNext)
                        {
                            writer.Write(',');
                        }
                        else
                        {
                            IsNext = true;
                        }
                        this.SerializeObject(objea, writer);
                    }
                    writer.Write(']');
                }
                else
                {
                    writer.Write('{');
                    PropertyInfo[] propertys = type.GetProperties();
                    bool IsEcho = false;
                    if (this.IsSerializer)
                    {
                        IsEcho = true;
                        writer.Write("\"{1}\":\"{0}\"", type.FullName, Constructor);
                    }
                    for (int i = 0; i < propertys.Length; i++)
                    {
                        var prop = propertys[i];
                        if (prop.GetIndexParameters().Length > 0 || prop.GetAccessors()[0].IsStatic)
                        {
                            continue;
                        }
                        Type ptype = prop.PropertyType;
                        if (this.IsSerializer)
                        {
                            goto Echo;
                        }
                        else if (ptype.IsValueType || ptype.IsPrimitive || ptype.Equals(typeof(System.String)))
                        {
                            goto Echo;
                        }
                        else if (prop.GetCustomAttributes(typeof(JSONAttribute), true).Length > 0
                              ||
                              ptype.GetCustomAttributes(typeof(JSONAttribute), true).Length > 0)
                        {
                            goto Echo;


                        }
                        else if (prop.CanWrite == false && prop.CanRead)
                        {

                            goto Echo;
                        }
                        continue;
                    Echo:
                        if (this.SerializeProperty(obj, prop, writer, IsEcho))
                        {
                            IsEcho = true;
                        }
                    }
                    writer.Write('}');

                }
            }
            else
            {
                writer.Write("\"\"");
            }
        }
        private bool SerializeProperty(object obj, PropertyInfo property, System.IO.TextWriter writer, bool IsEcho)
        {
            if (property.GetIndexParameters().Length == 0)
            {
                var ov = property.GetValue(obj, null);
                if (ov != null)
                {
                    if (IsEcho) writer.Write(',');

                    SerializeObject(property.Name, writer);
                    writer.Write(':');
                    SerializeObject(ov, writer);
                    return true;
                }
            }
            return false;
        }


    }
}
