using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using UMC.Net;
using System.IO;
using System.Collections;

namespace UMC.Web
{
    /// <summary>
    /// 交互上下文
    /// </summary>
    public class WebContext
    {


        internal WebRuntime runtime;

        internal protected virtual void OnInit(WebClient client)
        {
        }
        internal void Init(WebRuntime runtime)
        {
            this.runtime = runtime;
        }
        /// <summary>
        /// 触发关闭客户端事件
        /// </summary>
        public void Close()
        {
            this.Response.ClientEvent |= (WebEvent.Close | WebEvent.Reset);
            this.End();
        }
        /// <summary>
        /// 清除指定的客户端事件
        /// </summary>
        /// <param name="cEvent"></param>
        public void ClearEvent(WebEvent cEvent)
        {
            this.Response.ClientEvent ^= this.Response.ClientEvent & cEvent;
            this.Response.Headers.Remove(cEvent.ToString());
        }
        /// <summary>
        /// 向发送客户端发送数据事情
        /// </summary>
        /// <param name="data">发送的数据</param>
        /// <param name="endResponse"></param>
        public void Send(WebMeta data, bool endResponse)
        {
            WebResponse response = this.Response;
            if (data.ContainsKey("type") && data["type"] == "UI.Event")
            {
                if (runtime.Client.UIEvent != null)
                {

                    var click = runtime.Client.UIEvent;
                    var key = data["key"];
                    if (String.Equals(key, "Click") && data.GetDictionary()["value"] is ListItem)
                    {
                        runtime.Client.UIEvent = null;
                        runtime.Client.Session.Storage(new Hashtable(), this);
                        var value = data.GetDictionary()["value"] as ListItem;
                        var objValue = click.Send();
                        if (objValue is Hashtable)
                        {
                            var val = objValue as Hashtable;
                            var em = val.GetEnumerator();

                            while (em.MoveNext())
                            {
                                if (String.Equals(em.Value, "Value"))
                                {
                                    val[em.Key] = value.Value;
                                    val[String.Format("{0}_Text", em.Key)] = value.Text;
                                    //val.Put(em.Key as string, value.Value);
                                    break;
                                }
                            }
                            response.Redirect(click.Model, click.Command, new WebMeta(val), false);
                        }
                        else if (objValue is WebMeta)
                        {
                            var val = objValue as WebMeta;
                            var em = val.GetDictionary().GetEnumerator();

                            while (em.MoveNext())
                            {
                                if (String.Equals(em.Value, "Value"))
                                {
                                    val.Put(em.Key as string, value.Value);
                                    val[String.Format("{0}_Text", em.Key)] = value.Text;
                                    break;
                                }
                            }

                            response.Redirect(click.Model, click.Command, val, false);
                        }
                        else
                        {
                            response.Redirect(click.Model, click.Command, value.Value, false);

                        }
                    }

                }
            }
            response.ClientEvent |= WebEvent.DataEvent;
            if (response.Headers.ContainsKey("DataEvent"))
            {
                var ts = response.Headers.GetDictionary()["DataEvent"];
                if (ts is WebMeta)
                {
                    response.Headers.Set("DataEvent", (WebMeta)ts, data);

                }
                else if (ts is IDictionary)
                {
                    response.Headers.Set("DataEvent", new WebMeta((IDictionary)ts), data);

                }
                else if (ts is Array)
                {
                    var ats = new System.Collections.ArrayList();
                    ats.AddRange((Array)ts);
                    ats.Add(data);

                    response.Headers.Set("DataEvent", (WebMeta[])ats.ToArray(typeof(WebMeta)));
                }
                else
                {
                    response.Headers.Set("DataEvent", data);
                }

            }
            else
            {

                response.Headers.Set("DataEvent", data);
            }
            if (endResponse)
            {
                response.ClientEvent ^= response.ClientEvent & WebEvent.Normal;
                this.End();
            }

        }

        /// <summary>
        /// 向发送客户端发送数据事情
        /// </summary>
        /// <param name="type">数据事件</param>
        /// <param name="data">发送的数据</param>
        /// <param name="endResponse"></param>
        public void Send(String type, WebMeta data, bool endResponse)
        {
            this.Send(data.Put("type", type), endResponse);
        }
        /// <summary>
        /// 向发送客户端发送数据事情
        /// </summary>
        /// <param name="type">数据事件</param>
        /// <param name="endResponse"></param>
        public void Send(String type, bool endResponse)
        {
            WebMeta data = new WebMeta();
            Send(type, data, endResponse);
        }




        /// <summary>
        /// 触发客户端从新获取可用命令
        /// </summary>
        public void OnReset()
        {
            this.Response.ClientEvent |= WebEvent.Reset;
        }
        /// <summary>
        /// 处理完最后一个Actively事件
        /// </summary>
        internal protected virtual void Complete()
        {

        }

        /// <summary>
        /// 当前的会话上下文
        /// </summary>
        //public static WebContext Current
        //{
        //    get
        //    {
        //        return WebRuntime.Current.Context;//as POSContext;
        //    }
        //}
        /// <summary>
        /// 当前的正在处理路线
        /// </summary>
        public WebFlow CurrentFlow
        {

            get
            {
                return runtime.CurrentFlow;//as POSContext;
            }
        }
        public UMC.Security.AccessToken Token
        {

            get
            {
                return runtime.Client.Token;//as POSContext;
            }
        }
        /// <summary>
        /// 当前正在处理活动
        /// </summary>
        public WebActivity CurrentActivity
        {

            get
            {
                return runtime.CurrentActivity;//as POSContext;
            }
        }
        /// <summary>
        /// 当前正在处理的POSFlowFactory
        /// </summary>
        public IWebFactory FlowFactory
        {
            get
            {
                return runtime.CurrentFlowFactory;
            }
        }

        /// <summary>
        /// 路模块共享的数据键
        /// </summary>
        public System.Collections.IDictionary Items
        {
            get
            {
                return runtime.Items;
            }
        }



        /// <summary>
        /// 终止当前请求，并返回终端
        /// </summary>
        public void End()
        {
            throw new WebAbortException();
        }
        /// <summary>
        /// 请求信息
        /// </summary>
        public WebRequest Request
        {
            get
            {
                return runtime.Request;
            }
        }
        /// <summary>
        /// 响应信息
        /// </summary>
        public WebResponse Response
        {
            get
            {
                return runtime.Response;
            }
        }
    }
}
