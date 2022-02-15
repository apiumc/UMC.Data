using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Web
{
    /// <summary>
    /// UMC服务注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class MappingAttribute : System.Attribute
    {
        /// <summary>
        /// 用于注册WebActivity
        /// </summary>
        public MappingAttribute(String model, String cmd)
        {
            this.Model = model;
            this.Command = cmd;
        }
        /// <summary>
        /// 用于注册IWebFactory  标注程序集
        /// </summary>
        public MappingAttribute()
        {

        }
        /// <summary>
        /// 用于注册WebFlow
        /// </summary>
        /// <param name="model"></param>
        public MappingAttribute(String model)
        {
            this.Model = model;

        }

        public String Model
        {
            get;
            private set;
        }
        public String Command
        {
            get;
            private set;
        }
        public String Desc
        {
            get; set;
        }
        public WebAuthType Auth
        {
            get; set;
        }
        /// <summary>
        /// 权重，默认0，在同等指令下，执行权重高的指令
        /// </summary>
        public int Weight
        {
            get; set;
        }
        /// <summary>
        /// 功能分类
        /// </summary>
        public int Category { get; set; }

        
    }
}
