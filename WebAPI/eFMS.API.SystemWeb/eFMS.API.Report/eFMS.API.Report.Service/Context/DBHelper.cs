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
    public class DBHelper
    {
        public static string ConnectionString { get; set; }
        public static string MongoDBConnectionString { get; set; }
    }
}
