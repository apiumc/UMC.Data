using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data.Sql
{
    public interface IScript
    {
        Script SQL
        {
            get;
        }
    }
    public class Script
    {
        public string Text
        {
            get;
            private set;
        }
        public object[] Arguments
        {
            get;
            private set;
        }
        private Script()
        {
        }
        public void Reset(string text, params object[] objs)
        {
            this.Text = text; this.Arguments = objs;
        }
        public static Script Create(string text, params object[] args)
        {
            var sc = new Script();
            sc.Arguments = args;
            sc.Text = text;
            return sc;
        }
    }
}
