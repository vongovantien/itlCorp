using ITL.NetCore.Common.Items;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITL.NetCore.Connection.NoSql
{
    public class MongoDbHelper
    {
        private static MongoClient mogoClient;
        private static IMongoDatabase mongodb;
        public static IMongoDatabase GetDatabase()
        {
            if (mogoClient == null)
            {
                //mogoClient = new MongoClient("mongodb://tamphan:tamphan123456@ds211724.mlab.com:11724/efms");
                mogoClient = new MongoClient("mongodb://localhost:27017/fmslog");
            }
            //mongodb = mogoClient.GetDatabase("efms");
            mongodb = mogoClient.GetDatabase("fmslog");
            return mongodb;
        }

        public static void Insert(string collectionName, object model)
        {
            mongodb.GetCollection<object>(collectionName).InsertOne(model);
        }
        public static void InsertMany(string collectionName, List<object> model)
        {
            mongodb.GetCollection<object>(collectionName).InsertMany(model);
        }
        public static void Delete(string collectionName, ItemLog model)
        {
            var filter = Builders<ItemLog>.Filter.Where(e => model.Id == e.Id);
            var result = mongodb.GetCollection<ItemLog>(collectionName).FindOneAndDelete(filter);
        }
    }
}
