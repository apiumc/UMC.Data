using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UMC.Data;
using UMC.Net;

namespace UMC.Web.Activity
{
    /// <summary>
    /// CSV文件导入
    /// </summary>
    public abstract class CSVActivity : WebActivity
    {
        public abstract string Title
        {
            get;
        }
        public abstract string[] Columns
        {
            get;
        }
        protected abstract bool Do(CSV.Log log, int rowIndex, Hashtable value);
        public override void ProcessActivity(WebRequest request, WebResponse response)
        {
            var Settings = this.AsyncDialog("Settings", g =>
            {
                if (request.SendValues != null && request.SendValues.ContainsKey("media_id"))
                {
                    return this.DialogValue(new WebMeta().Put("media_id", request.SendValues["media_id"]));
                }
                var from = new Web.UIFormDialog() { Title = this.Title ?? "文本数据导入" };

                from.AddFile("选择csv文件", "media_id", String.Empty).Put("Accept", "text/csv");
                from.AddRadio("文件编码", "Encoding").Put("中文操作系统标准(GB2312)", "gb2312").Put("英文操作系统标准(UTF8)", "utf-8", true);
                var opts = from.AddTextValue("必须列名");
                foreach (var s in Columns)
                {
                    opts.Add(s, "必须字段");
                }
                from.Submit("确认上传", $"{request.Model}.{request.Command}");
                return from;

            });
            var media_id = Settings["media_id"];
            if (media_id.EndsWith(".csv") == false)
            {
                this.Prompt("请上传csv文本文件");

            }

            var Key = this.AsyncDialog("Key", Utility.Guid(Guid.NewGuid()));
            var pathFile = this.AsyncDialog("File", g =>
            {
                string path2 = UMC.Data.Utility.MapPath(String.Format("App_Data\\Static\\TEMP\\{0}\\", Utility.GetRoot(request.Url)));

                var filename = String.Format("{0}{1}.tmp", path2, Key);
                using (var stream = Utility.Writer(filename, false))
                {
                    new Uri(media_id).WebRequest().Get().ReadAsStream(stream);
                    stream.Close();
                }


                return this.DialogValue(filename);

            });
            if (pathFile.EndsWith(".csv"))
            {
                var pad = new Uri(request.Url, String.Format("/TEMP/{0}", pathFile));
                this.Context.Send(new Web.WebMeta().Put("type", "OpenUrl").Put("value", pad.AbsoluteUri), true);
            }
            var columnSettings = this.AsyncDialog("Columns", g =>
            {
                var reader = new System.IO.StreamReader(pathFile, Encoding.GetEncoding(Settings["Encoding"] ?? "utf-8"));
                var cinex = 0;
                var from = new Web.UIFormDialog() { Title = "核对字段" };
                var csvColumns = CSV.FromCsvLine(reader.ReadLine());
                reader.Close();
                foreach (var c in this.Columns)
                {
                    var opts = from.AddSelect(c, "Col_" + cinex).Put("请选择", "");
                    for (var i = 0; i < csvColumns.Length; i++)
                    {
                        opts.Add(String.Format("{1}(第{0}列)", i + 1, csvColumns[i]), i.ToString(), String.Equals(csvColumns[i], c, StringComparison.CurrentCultureIgnoreCase));
                    }
                    cinex++;
                }
                from.Submit("确认通过");
                return from;

            });
            var indexs = new List<int>();
            var maxIndex = -1;
            for (var i = 0; i < this.Columns.Length; i++)
            {
                var cindex = Convert.ToInt32(columnSettings["Col_" + i]);
                if (maxIndex < cindex)
                {
                    maxIndex = cindex;
                }
                indexs.Add(cindex);
            }
            if (indexs.Count == 0)
            {
                this.Prompt("无有效的字段");
            }

            var userName = this.Context.Token.Username;

            var log = new CSV.Log(Key, String.Format("开始{0}", this.Title ?? "文本数据导入"));
            Data.Reflection.Start(() =>
                {
                    int rowIndex = 1;
                    var reader = new System.IO.StreamReader(pathFile, Encoding.GetEncoding(Settings["Encoding"] ?? "utf-8"));
                    System.IO.FileStream file = System.IO.File.Open(String.Format("{0}.csv", pathFile), System.IO.FileMode.Create);


                    try
                    {
                        var writer = new System.IO.StreamWriter(file, Encoding.UTF8);
                        var total = 0;
                        var now = DateTime.Now;


                        var header = reader.ReadLine();
                        writer.WriteLine(header);

                        int okindex = 0;

                        CSV.EachRow(reader, data =>
                        {
                            rowIndex++;
                            if (data.Length <= maxIndex)
                            {
                                log.Error(String.Format("第{0}行 数据无效", rowIndex));
                                if (okindex + 10 < rowIndex)
                                {
                                    throw new ArgumentException(String.Format("连续超过10条无效的数据"));
                                }
                                return;
                            }
                            var hash = new Hashtable();
                            for (var i = 0; i < indexs.Count; i++)
                            {
                                int index = indexs[i];
                                if (data.Length > index)
                                {
                                    if (String.IsNullOrEmpty(data[index]))
                                    {

                                        log.Error(String.Format("第{0}行 {1}列数据为空", rowIndex, this.Columns[i]));
                                        if (okindex + 10 < rowIndex)
                                        {
                                            throw new ArgumentException(String.Format("连续超过10条无效的数据"));
                                        }
                                        return;
                                    }

                                }
                                else
                                {
                                    log.Error(String.Format("第{0}行 无{1}列数据", rowIndex, this.Columns[i]));
                                    if (okindex + 10 < rowIndex)
                                    {
                                        throw new ArgumentException(String.Format("连续超过10条无效的数据"));
                                    }
                                    return;

                                }
                                hash[this.Columns[i]] = data[index];
                            }
                            if (Do(log, rowIndex, hash))
                            {
                                total++;
                                okindex = rowIndex;
                            }
                            else
                            {
                                foreach (var d in data)
                                {
                                    UMC.Data.CSV.CSVFormat(writer, d);
                                    writer.Write(",");
                                }
                                writer.WriteLine();
                                writer.Flush();
                            }
                            if (okindex + 10 < rowIndex)
                            {
                                new ArgumentException(String.Format("连续超过10条无效的数据"));
                            }
                        });
                        writer.Flush();
                        writer.Close();
                        log.End("导入数据完成");
                        log.Info(String.Format("导入成功{0}条", total));
                        log.Info(String.Format("用时{0}", DateTime.Now - now));

                    }
                    catch (Exception ex)
                    {
                        log.End(String.Format("在{0}行导入失败", rowIndex));
                        log.Info(ex.Message);

                    }
                    finally
                    {
                        file.Close();
                        reader.Close();
                        log.Close();
                    }

                });//.Start();
            this.Context.Send($"{request.Model}.{request.Command}", false);
            this.Context.Send(new UISectionBuilder("System", "Log", new WebMeta("Key", Key))
                    .Builder(), true);
        }

    }

}
