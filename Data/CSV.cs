using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.IO;
using System.Collections;

namespace UMC.Data
{
    /// <summary> 
    /// csv解析工具
    /// </summary>
    public sealed class CSV
    {
        public class Log
        {
            String path;
            public Log(String root, String key, String msg)
            {
                string path = UMC.Data.Utility.MapPath(String.Format("App_Data\\Static\\TEMP\\{0}\\", root));
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                this.path = String.Format("{0}{1}.csv", path, key);
                Writer("START", msg);
            }

            public Log(String msg)
            {

                Writer("START", msg);
            }
            public void Close()
            {

            }

            public void Error(params object[] objs)
            {
                Writer("ERROR", objs);
            }
            void Writer(string name, params object[] objs)
            {
                var ks = new List<object>();
                ks.Add(name);
                ks.AddRange(objs);
                if (String.IsNullOrEmpty(path))
                {
                    WriteLine(Console.Out, ks.ToArray());
                }
                else
                {
                    using (FileStream stream = new FileStream(this.path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        var writer = new System.IO.StreamWriter(stream);
                        WriteLine(writer, ks.ToArray());
                        writer.Flush();
                        writer.Close();

                    }

                }
            }

            public void Info(params object[] objs)
            {
                Writer("INFO", objs);

            }
            public void Debug(params object[] objs)
            {
                Writer("DEBUG", objs);



            }
            public void End(params object[] objs)
            {
                Writer("END", objs);

            }
        }
        /// <summary>
        /// 导出CSV文件
        /// </summary>
        /// <param name="dr">只读数据集</param>
        /// <param name="headers">参数</param>
        /// <returns>返回一个文件</returns>
        public static string ExportCSV(System.Data.IDataReader dr, System.Collections.Hashtable headers)
        {
            string randomFile = System.IO.Path.GetRandomFileName();
            string path = UMC.Data.Utility.MapPath("App_Data\\Static\\TEMP\\");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            using (System.IO.FileStream file = System.IO.File.Open(path + randomFile, System.IO.FileMode.Create))
            {
                var writer = new System.IO.StreamWriter(file, Encoding.Default);
                try
                {
                    UMC.Data.CSV.ExportCSV(writer, dr, headers);

                    return randomFile;
                }
                finally
                {
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// 导出CSV文件
        /// </summary>
        /// <param name="tab">表格</param>
        /// <returns>返回一个文件</returns>
        public static string ExportCSV(System.Data.DataTable tab)
        {
            string randomFile = System.IO.Path.GetRandomFileName();
            string path = UMC.Data.Utility.MapPath("App_Data\\Static\\TEMP\\");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            using (System.IO.FileStream file = System.IO.File.Open(path + randomFile, System.IO.FileMode.Create))
            {
                var writer = new System.IO.StreamWriter(file, Encoding.Default);
                try
                {
                    //writer.Write(
                    UMC.Data.CSV.ExportCSV(writer, tab);

                    return randomFile;
                }
                finally
                {
                    writer.Close();
                }
            }
        }
        /// <summary>
        /// 时间格式
        /// </summary>
        public const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private CSV() { }
        /// <summary>
        /// 将CSV数据转换为DataTable
        /// </summary>
        /// <param name="reader">只读文本流</param>
        /// <param name="isRowHead">是否将第一行作为字段名</param>
        /// <returns></returns>
        public static DataTable ToDataTable(TextReader reader, bool isRowHead)
        {
            DataTable dt = null;

            dt = new DataTable();
            string csvRow = ReadLine(reader);


            object[] csvColumns = null;
            if (csvRow != null)
            {
                //第一行作为字段名,添加第一行记录并删除csvRows中的第一行数据
                if (isRowHead)
                {
                    csvColumns = FromCsvLine(csvRow);
                    for (int i = 0; i < csvColumns.Length; i++)
                    {
                        dt.Columns.Add(csvColumns[i].ToString());
                    }

                    csvRow = ReadLine(reader);
                }

                while (csvRow != null)
                {
                    csvColumns = FromCsvLine(csvRow);
                    if (dt.Columns.Count < csvColumns.Length)
                    {
                        int columnCount = csvColumns.Length - dt.Columns.Count;
                        for (int c = 0; c < columnCount; c++)
                        {
                            dt.Columns.Add();
                        }
                    }
                    dt.Rows.Add(csvColumns);
                    csvRow = ReadLine(reader);
                }
            }


            return dt;
        }
        public static void EachRow(TextReader reader, Action<String[]> action)
        {
            string csvRow = ReadLine(reader);

            while (csvRow != null)
            {
                var rows = FromCsvLine(csvRow);

                action(rows);

                csvRow = ReadLine(reader);
            }

        }
        public static void Each(TextReader reader, Action<IDictionary> action)
        {
            string csvRow = ReadLine(reader);


            if (csvRow != null)
            {
                //第一行作为字段名,添加第一行记录并删除csvRows中的第一行数据

                var csvColumns = FromCsvLine(csvRow);


                csvRow = ReadLine(reader);


                while (csvRow != null)
                {
                    var rows = FromCsvLine(csvRow);
                    var hash = new Hashtable();
                    for (var i = 0; i < csvColumns.Length; i++)
                    {
                        if (i < rows.Length)
                        {
                            if (String.IsNullOrEmpty(csvColumns[i]) == false)
                            {
                                hash[csvColumns[i]] = rows[i];
                            }
                        }
                    }
                    action(hash);

                    csvRow = ReadLine(reader);
                }
            }


        }

        /// <summary>
        /// 从CSV文件读取数据
        /// </summary>
        /// <param name="file">CSV文件名的路径</param>
        /// <param name="isRowHead">是否将第一行作为字段名</param>
        /// <returns></returns> 
        public static DataTable ToDataTable(string file, bool isRowHead)
        {
            using (var reader = new System.IO.StreamReader(file))
            {
                return ToDataTable(reader, isRowHead);
            }

        }


        /// <summary>
        /// 把数据表以CSV格式导出到文本流
        /// </summary>
        /// <param name="file">导出文件名</param>
        /// <param name="dr">数据表</param>
        /// <param name="adrs">adrs</param>
        public static void ExportCSV(string file, System.Data.IDataReader dr, params System.Action<IDataRecord>[] adrs)
        {
            var wirter = new System.IO.StreamWriter(file, false, Encoding.ASCII);
            ExportCSV(wirter, dr, new System.Collections.Hashtable(), adrs);
            wirter.Flush();
            wirter.Close();
        }
        /// <summary>
        /// 把数据表以CSV格式导出到文本流
        /// </summary>
        /// <param name="file">导出文件名</param>
        /// <param name="dr">数据表</param>
        /// <param name="headers">列头对应字典</param>
        /// <param name="adrs">adrs</param>
        public static void ExportCSV(string file, System.Data.IDataReader dr, System.Collections.Hashtable headers, params System.Action<IDataRecord>[] adrs)
        {
            var wirter = new System.IO.StreamWriter(file, false, Encoding.ASCII);
            ExportCSV(wirter, dr, headers, adrs);
            wirter.Flush();
            wirter.Close();
        }

        /// <summary>
        /// 把数据表以CSV格式导出到文本流
        /// </summary>
        /// <param name="writer">只写文本流</param>
        /// <param name="dr">数据表</param>
        /// <param name="adrs">adrs</param>
        public static int ExportCSV(TextWriter writer, System.Data.IDataReader dr, params System.Action<IDataRecord>[] adrs)
        {
            return ExportCSV(writer, dr, new System.Collections.Hashtable(), adrs);
        }
        /// <summary>
        /// 把数据表以CSV格式导出到文本流
        /// </summary>
        /// <param name="writer">只写文本流</param>
        /// <param name="dr">数据表</param>
        /// <param name="headers">列头对应字典</param>
        /// <param name="adrs">adrs</param>
        public static int ExportCSV(TextWriter writer, System.Data.IDataReader dr, System.Collections.Hashtable headers, params System.Action<IDataRecord>[] adrs)
        {
            int cCount = dr.FieldCount;
            for (int i = 0; i < cCount; i++)
            {
                if (i != 0)
                    writer.Write(',');
                string field = dr.GetName(i);
                object obj = GetHashtableValue(headers, field);
                if (obj == null)
                    writer.Write(CSVFormat(field));
                else
                    writer.Write(CSVFormat(obj.ToString()));
            }
            writer.WriteLine();
            int count = 0;
            while (dr.Read())
            {
                count++;
                for (int i = 0; i < cCount; i++)
                {
                    if (i != 0)
                        writer.Write(',');
                    writer.Write(CSVFormat(dr.GetValue(i)));
                }
                writer.WriteLine();// ('\r');

                for (var i = 0; i < adrs.Length; i++)
                {
                    adrs[i](dr);
                }
            }
            writer.Flush();
            return count;

        }

        /// <summary>
        /// 把数据表以CSV格式导出到文件
        /// </summary>
        /// <param name="file">导出文件名</param>
        /// <param name="table">数据表</param>
        public static void ExportCSV(string file, System.Data.DataTable table)
        {

            var wirter = new System.IO.StreamWriter(file, false, Encoding.Default);
            ExportCSV(wirter, table);
            wirter.Flush();
            wirter.Close();

        }
        /// <summary>
        /// 把数据表以CSV格式导出到文本流
        /// </summary>
        /// <param name="writer">只写文本流</param>
        /// <param name="table">数据表</param>
        public static void ExportCSV(TextWriter writer, System.Data.DataTable table)
        {

            int cCount = table.Columns.Count;
            for (int i = 0; i < cCount; i++)
            {
                if (i != 0)
                {
                    writer.Write(',');
                }
                if (String.IsNullOrEmpty(table.Columns[i].Caption))
                {
                    writer.Write(CSVFormat(table.Columns[i].ColumnName));
                }
                else
                {
                    writer.Write(CSVFormat(table.Columns[i].Caption));
                }
            }
            writer.Write("\r\n");

            foreach (System.Data.DataRow dr in table.Rows)
            {
                for (int i = 0; i < cCount; i++)
                {
                    if (i != 0)
                    {
                        writer.Write(',');
                    }
                    writer.Write(CSVFormat(dr[i]));
                }
                writer.Write("\r\n");
            }
            writer.Flush();
        }

        static object GetHashtableValue(System.Collections.Hashtable hash, string Key)
        {
            if (hash.ContainsKey(Key))
            {
                return hash[Key];
            }
            System.Collections.IDictionaryEnumerator enumer = hash.GetEnumerator();
            while (enumer.MoveNext())
            {
                if (enumer.Key.ToString().ToLower() == Key.ToLower())
                {
                    return enumer.Value;
                }
            }
            return null;
        }
        /// <summary>
        /// 把List<T>导出的CSV文件
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="writer">文本写入流</param>
        /// <param name="list">实体类型对象列表</param>
        /// <param name="headers">列头对应表</param>
        /// <returns>返回导出文件名</returns>
        public static void ExportCSV<T>(TextWriter writer, List<T> list, System.Collections.Hashtable headers)
        {

            Type type = typeof(T);
            System.Reflection.PropertyInfo[] ps = type.GetProperties();


            int cCount = ps.Length;
            for (int i = 0; i < cCount; i++)
            {
                if (i != 0)
                {
                    writer.Write(',');
                }

                string field = ps[i].Name;
                object obj = GetHashtableValue(headers, field);
                if (obj == null)
                {

                    writer.Write(CSVFormat(field));
                }
                else
                {
                    writer.Write(CSVFormat(obj.ToString()));
                }
            }
            writer.Write("\r\n");

            foreach (T t in list)
            {
                for (int i = 0; i < cCount; i++)
                {
                    if (i != 0)
                    {
                        writer.Write(',');
                    }
                    writer.Write(CSVFormat(ps[i].GetValue(t, null)));
                }
                writer.Write("\r\n");
            }
            writer.Flush();
        }
        public static string CSVFormat(object obj)
        {
            StringBuilder sb = new StringBuilder();

            if (obj is DateTime)
            {
                sb.AppendFormat("{0:" + DateFormat + "}", obj);
            }
            else
            {
                sb.Append(obj);
            }

            sb.Replace("\"", "\"\"").Replace("\r\n", "\n");
            string str = sb.ToString();
            if (str.IndexOf('"') > -1)
            {
                sb.Insert(0, "\"");
                sb.Append('"');
            }
            else if (str.IndexOf('\n') > -1)
            {
                sb.Insert(0, "\"");
                sb.Append('"');
            }
            else if (str.IndexOf(',') > -1)
            {
                sb.Insert(0, "\"");
                sb.Append('"');
            }
            else if (str.StartsWith(" "))
            {
                sb.Insert(0, "\"");
                sb.Append('"');
            }
            else if (str.EndsWith(" "))
            {
                sb.Insert(0, "\"");
                sb.Append('"');
            }
            else if (obj is string)
            {
                if (obj.ToString() == String.Empty)
                {
                    sb.Insert(0, "\"");
                    sb.Append('"');
                }
            }
            return sb.ToString();

        }
        public static void WriteLine(TextWriter writer, params object[] objs)
        {

            for (var i = 0; i < objs.Length; i++)
            {
                if (i > 0)
                    writer.Write(",");

                UMC.Data.CSV.CSVFormat(writer, objs[i]);
            }
            writer.WriteLine();
        }
        public static void CSVFormat(TextWriter writer, object obj)
        {

            if (obj is DateTime)
            {
                writer.Write("{0:" + DateFormat + "}", obj);
            }
            else if (obj is string)
            {
                string str = obj as string;
                str = str.Replace("\"", "\"\"").Replace("\r\n", "\n");

                writer.Write('"');
                writer.Write(str);
                writer.Write('"');
            }
            else
            {
                writer.Write(obj);
            }


        }
        /// <summary>
        /// 把CSV格式的只读文本流转化为DataReader
        /// </summary>
        /// <param name="reader">CSV格式的文本流</param>
        /// <returns></returns>
        public static System.Data.IDataReader ToDataReader(System.IO.TextReader reader)
        {
            return new CSVDataReader(reader);
        }
        /// <summary>
        /// 从CSV格林中读一行数据
        /// </summary>
        /// <param name="reader">CSV格式的文本流</param>
        /// <returns></returns>
        public static string ReadLine(TextReader reader)
        {
            //StringBuilder builder = new StringBuilder();
            // bool IsRe = false;
            //Environment.NewLine
            return reader.ReadLine();
            //while (true)
            //{
            //    int num = reader.Read();
            //    switch (num)
            //    {
            //        case -1:
            //            if (builder.Length > 0)
            //            {
            //                return builder.ToString();
            //                //TextReader.
            //            }
            //            return null;

            //        case 13:
            //        case 10:
            //            if ((num == 13) && (reader.Peek() == 10))
            //            {
            //                reader.Read();
            //                return builder.ToString();
            //            }
            //            builder.Append('\r');
            //            break;
            //    }
            //    builder.Append((char)num);
            //}
        }





        /// <summary>
        /// 解析一行CSV数据
        /// </summary>
        /// <param name="csv">csv数据行</param>
        /// <returns>返回DBNULL值和字符串</returns>
        public static string[] FromCsvLine(string csv)
        {
            List<String> csvLiAsc = new List<String>();
            List<String> csvLiDesc = new List<String>();

            if (!string.IsNullOrEmpty(csv))
            {
                //顺序超找
                int lastIndex = 0;
                int quotCount = 0;
                //剩余的字符串

                for (int i = 0; i < csv.Length; i++)
                {
                    if (csv[i] == '"')
                    {
                        if (i > 0)
                        {
                            if ((csv[i - 1] != '\\'))
                            {
                                quotCount++;
                            }

                        }
                        else
                            quotCount++;
                    }
                    else if (csv[i] == ',' && quotCount % 2 == 0)
                    {
                        csvLiAsc.Add(ReplaceQuote(csv.Substring(lastIndex, i - lastIndex)));
                        lastIndex = i + 1;
                    }
                    if (i == csv.Length - 1 && lastIndex < csv.Length)
                    {
                        csvLiAsc.Add(ReplaceQuote(csv.Substring(lastIndex))); //;// i - lastIndex + 1);
                    }
                }
            }

            return csvLiAsc.ToArray();
        }
        /// <summary>
        ///// 反转字符串
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //private static string Reverse(string str)
        //{

        //    var sb = new StringBuilder();
        //    for (int i = str.Length - 1; i > -1; i--)
        //    {
        //        sb.Append(str[i]);
        //    }
        //    return sb.ToString(); ;
        //}
        /// <summary>
        /// 替换CSV中的双引号转义符为正常双引号,并去掉左右双引号
        /// </summary>
        /// <param name="csvValue">csv格式的数据</param>
        /// <returns></returns>
        private static String ReplaceQuote(string csvValue)
        {
            if (csvValue == String.Empty)
            {
                return null;
            }
            string rtnStr = csvValue;
            if (!string.IsNullOrEmpty(csvValue))
            {
                switch (csvValue[0])
                {
                    case '"':
                        switch (rtnStr.Length)
                        {
                            case 1:
                                return String.Empty;
                            case 2:
                                return String.Empty;
                            default:
                                rtnStr = rtnStr.Substring(1, rtnStr.Length - 2);
                                break;
                        }
                        break;

                }

                rtnStr = rtnStr.Replace("\"\"", "\"");
            }
            return rtnStr;

        }
        #region CSVDataReader
        class CSVDataReader : System.Data.IDataReader
        {
            System.Collections.Generic.Dictionary<string, int> hask = new Dictionary<string, int>();
            TextReader TextReader;
            string csvRow = null;
            object[] csvColumns = null;
            public CSVDataReader(TextReader reader)
            {
                TextReader = reader;
                csvRow = CSV.ReadLine(reader);
                if (csvRow != null)
                {
                    csvColumns = CSV.FromCsvLine(csvRow);
                    for (var i = 0; i < csvColumns.Length; i++)
                    {
                        hask[csvColumns[i].ToString()] = i;
                    }
                }
            }

            #region IDataReader Members

            void IDataReader.Close()
            {
                TextReader.Close();
            }

            int IDataReader.Depth
            {
                get { return 1; }
            }

            DataTable IDataReader.GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            bool IDataReader.IsClosed
            {
                get { return false; }
            }

            bool IDataReader.NextResult()
            {
                return false;
            }

            bool IDataReader.Read()
            {
                csvRow = CSV.ReadLine(TextReader);

                csvColumns = CSV.FromCsvLine(csvRow);
                return csvRow != null;
            }

            int IDataReader.RecordsAffected
            {
                get { throw new NotImplementedException(); }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose()
            {
            }

            #endregion

            #region IDataRecord Members

            int IDataRecord.FieldCount
            {
                get { return hask.Count; }
            }

            bool IDataRecord.GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            byte IDataRecord.GetByte(int i)
            {
                throw new NotImplementedException();
            }

            long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            char IDataRecord.GetChar(int i)
            {
                throw new NotImplementedException();
            }

            long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            IDataReader IDataRecord.GetData(int i)
            {
                throw new NotImplementedException();
            }

            string IDataRecord.GetDataTypeName(int i)
            {
                return typeof(string).Name;
            }

            DateTime IDataRecord.GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            decimal IDataRecord.GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            double IDataRecord.GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            Type IDataRecord.GetFieldType(int i)
            {
                return typeof(string);
            }

            float IDataRecord.GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            Guid IDataRecord.GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            short IDataRecord.GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            int IDataRecord.GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            long IDataRecord.GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            string IDataRecord.GetName(int i)
            {
                var em = hask.GetEnumerator();
                while (em.MoveNext())
                {
                    if (em.Current.Value == i)
                    {
                        return em.Current.Key;
                    }
                }
                return null;
            }

            int IDataRecord.GetOrdinal(string name)
            {
                if (this.hask.ContainsKey(name))
                {
                    var i = this.hask[name];
                    return i;
                }
                else
                {
                    return -1;
                }
            }

            string IDataRecord.GetString(int i)
            {
                return csvColumns[i] as string;
            }

            object IDataRecord.GetValue(int i)
            {
                if (csvColumns.Length > i)
                {
                    return csvColumns[i];
                }
                return null;
            }

            int IDataRecord.GetValues(object[] values)
            {
                Array.Copy(csvColumns, 0, values, 0, values.Length);
                return csvColumns.Length;
            }

            bool IDataRecord.IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            object IDataRecord.this[string name]
            {
                get
                {
                    if (this.hask.ContainsKey(name))
                    {
                        var i = this.hask[name];
                        if (csvColumns.Length > i)
                        {
                            return csvColumns[i];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            object IDataRecord.this[int i]
            {
                get
                {
                    if (csvColumns.Length > i)
                    {
                        return csvColumns[i];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            #endregion
        }
        #endregion

    }
}