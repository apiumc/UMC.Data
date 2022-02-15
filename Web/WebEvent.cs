using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Reflection;

namespace UMC.Web
{
    /// <summary>
    /// 客户端事件
    /// </summary>
    public enum WebEvent
    {
        /// <summary>
        /// 无事件
        /// </summary>
        None = 0,
        /// <summary>
        /// 提示
        /// </summary>
        Prompt = 1,
        /// <summary>
        /// 对话框
        /// </summary>
        Dialog = 2,
        /// <summary>
        /// 异步对话框
        /// </summary>
        AsyncDialog = 4,

        /// <summary>
        /// 从新获取可用命令
        /// </summary>
        Reset = 8,
        /// <summary>
        /// 单据交易完成
        /// </summary>
        Finish = 512,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 32,
        /// <summary>
        /// 客户数据事件
        /// </summary>
        DataEvent = 64,
        /// <summary>
        /// 客户端关闭退出
        /// </summary>
        Close = 128,
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 1024,
        /// <summary>
        /// 保存头部信息事件
        /// </summary>
        Header = 2048
    }
}
