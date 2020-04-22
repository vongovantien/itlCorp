using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;
using System.Data.SqlClient;

namespace eFMS.API.Setting.DL.Helpers
{
    public static class Helper
    {
        public static DataSet ExecuteDataSet(string connectString, string sqlString, params SqlParameter[] cmdParams)
        {
            using (SqlConnection conn = new SqlConnection(connectString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    if (cmdParams != null)
                    {
                        cmd.Parameters.AddRange(cmdParams);
                    }

                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    return ds;
                }
            }
        }
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (DataRow r in table.Rows)
                {
                    // add to the list
                    list.Add(CreateItemFromRow<T>(r));
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public static T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }
    }
}
