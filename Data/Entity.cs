using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Reflection;
namespace UMC.Data
{
    /// <summary>
    /// 实体配
    /// </summary>
    /// <typeparam name="E">实体类型</typeparam>
    /// <typeparam name="C">配置类型</typeparam>
    public sealed class Entity<E, C> where E : class
    {
        E entity;
        C config;
        public Entity(E entity, string config)
        {
            this.entity = entity;
            this.config = JSON.Deserialize<C>(config);
        }
        /// <summary>
        /// 配置的字段
        /// </summary>
        public string Field
        {
            get;
            set;
        }
        [JSON]
        public E Value
        {
            get
            {
                return this.entity;
            }
        }
        [JSON]
        public C Config
        {
            get
            {
                return this.config;
            }
        }
        /// <summary>
        /// 提交配置更新
        /// </summary>
        /// <param name="entity"></param>
        public void Commint(UMC.Data.Sql.IObjectEntity<E> entity)
        {
            if (String.IsNullOrEmpty(this.Field))
            {
                throw new ArgumentNullException("Field");
            }
            var hask = new System.Collections.Hashtable();
            hask[this.Field] = UMC.Data.JSON.Serialize(this.config);
            entity.Update(hask);
        }
    }
}