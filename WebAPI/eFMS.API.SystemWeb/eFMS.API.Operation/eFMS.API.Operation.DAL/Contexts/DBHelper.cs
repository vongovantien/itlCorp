using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbHelper
{
    public class DbHelper
    {
        public static string ConnectionString { get; set; }
        public static string MongoDBConnectionString { get; set; }
        public static DataTable FillDataTable(IDataReader reader)
        {
            DataTable dt = new DataTable();
            int fieldCount = reader.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                DataColumn dc = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                dt.Columns.Add(dc);
            }
            while (reader.Read())
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < fieldCount; i++)
                {
                    dr[i] = reader[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}