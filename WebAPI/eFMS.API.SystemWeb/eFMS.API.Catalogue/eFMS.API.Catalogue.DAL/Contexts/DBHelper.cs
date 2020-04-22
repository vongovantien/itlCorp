using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DbHelper
{
    public class DbHelper
    {
        public static string ConnectionString { get; set; }
        public static string MongoDBConnectionString { get; set; }

    }

}