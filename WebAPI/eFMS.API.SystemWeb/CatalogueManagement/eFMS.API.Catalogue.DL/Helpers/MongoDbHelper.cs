using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Helpers
{
    public static class MongoDbHelper
    {
        private static MongoClient mogoClient;
        private static IMongoDatabase mongodb;
        public static IMongoDatabase GetDatabase()
        {
            if(mogoClient == null)
            {
                mogoClient = new MongoClient("mongodb://tamphan:tamphan123456@ds211724.mlab.com:11724/efms");
            }
            mongodb = mogoClient.GetDatabase("efms");
            return mongodb;
        }
    }
}
