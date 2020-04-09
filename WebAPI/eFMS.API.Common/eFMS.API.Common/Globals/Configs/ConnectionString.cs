using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Common.Globals.Configs
{
    public class ConnectionString
    {
        public string MongoConnection { get; set; }
        public string MongoDatabase { get; set; }
        public string eFMSConnection { get; set; }
        public string RedisConnection { get; set; }
    }
}
