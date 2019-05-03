using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ITL.NetCore.Common.Items
{
    public class BasicItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public Dictionary<string, object> AnotherInfos = new Dictionary<string, object>();

        public string ValueText
        {
            get { return Value != null && (Value is string) ? Value.ToString() : ""; }
            set { Value = value; }
        }

        public int ValueInt
        {
            get { return Value != null && (Value is int || Value is Enum) ? (int)Value : 0; }
            set { Value = value; }
        }

        public decimal ValueDecimal
        {
            get { return Value != null && Value is decimal ? (decimal)Value : 0; }
            set { Value = value; }
        }

        public BasicItem()
        {
            Text = string.Empty;
        }

        public BasicItem(object value)
        {
            Text = value != null ? value.ToString() : "";
            Value = value;
        }

        public BasicItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public static List<BasicItem> GetListFromColumns(System.Data.DataTable dt)
        {
            List<BasicItem> l = new List<BasicItem>();
            if (dt != null)
            {
                foreach (DataColumn dc in dt.Columns)
                    l.Add(new BasicItem(dc.ColumnName));
            }
            return l;
        }

        public static List<BasicItem> GetListFromList(List<string> values)
        {
            return GetListOri(values.ToArray());
        }

        public static List<BasicItem> GetList(params object[] values)
        {
            return GetListOri(values);
        }

        public static List<BasicItem> GetListOri(object[] values)
        {
            List<BasicItem> l = new List<BasicItem>();
            foreach (object val in values)
                l.Add(new BasicItem(val));
            return l;
        }

    }
}