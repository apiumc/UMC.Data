using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
     
    /// <summary>
    /// 绑定指定的字段，如果没有定义默认对应的属性名
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : System.Attribute
    {
        /// <summary>
        /// 默认对应的属性名
        /// </summary>
        public FieldAttribute()
        {
            this.Insert = true;
            this.Select = true;
            this.Update = true;
            this.AutoField = false;
        }
        /// <summary>
        /// 标示属性对应的字段
        /// </summary>
        /// <param name="fName">对应字段名</param>
        public FieldAttribute(string fName)
            : base()
        {
            if (string.IsNullOrEmpty(fName))
            {
                throw new System.ArgumentNullException("fName");
            }
            this.Field = fName;
        }
        /// <summary>
        /// 标示属性对应的字段
        /// </summary>
        /// <param name="fName">对应字段名</param>
        /// <param name="isExpression">是否是表示示</param>
        /// <remarks>注意：如果是使用表达式就把对就的转化成与属性相同字段名</remarks>
        public FieldAttribute(string fName, bool isExpression)
            : this(fName)
        {
            this.IsExpression = isExpression;
            if (isExpression)
            {
                this.Update = false;
                this.Insert = false;
            }
        }

        /// <summary>
        /// 设置为自动增加列，则此列不可更新，也不可插入
        /// </summary>
        /// <param name="auto">是否是自动增长列</param>
        public FieldAttribute(bool auto)
        {
            this.Select = true;
            this.AutoField = true;
            if (auto)
            {
                this.Insert = false;
                this.Update = false;
            }
            else
            {
                this.Insert = true;
                this.Select = true;
            }
        }
        /// <summary>
        /// 是否是表达式
        /// </summary>
        public bool IsExpression
        {
            get;
            private set;
        }
        /// <summary>
        /// 绑定的字段名
        /// </summary>
        public string Field
        {
            get;
            set;
        }
        /// <summary>
        /// 当更新数据库时，是否更新此字段
        /// </summary>
        public bool Update
        {
            get;
            set;
        }
        /// <summary>
        /// 当查询数据库时，是不是把字段值赋于当前属性
        /// </summary>
        public bool Select
        {
            get;
            set;
        }
        /// <summary>
        /// 当插入数据库时，是不是把属性值填充到对应的字段中
        /// </summary>
        public bool Insert
        {
            get;
            set;
        }
        /// <summary>
        /// 是否是自动增长列
        /// </summary>
        public bool AutoField
        {
            get;
            private set;
        }


    }
    /// <summary>
    /// 定义实体对应的表名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        public TableAttribute()
        {
        }
        public TableAttribute(string tabName)
        {
            this.Name = tabName;
        }
        /// <summary>
        /// 表名，如果没有定义，默认对象的类型名
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }
}
