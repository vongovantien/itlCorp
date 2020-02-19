using eFMS.API.Common.Globals;
using MongoDB.Driver;
using System.Collections.Generic;

namespace eFMS.API.Common.NoSql
{
    public static class MongoDbHelper
    {
        private static MongoClient mogoClient;
        private static IMongoDatabase mongodb;
        public static IMongoDatabase GetDatabase(string connection)
        {
            if (mogoClient == null)
            {
                //mogoClient = new MongoClient("mongodb://tamphan:tamphan123456@ds211724.mlab.com:11724/efms");
                //mogoClient = new MongoClient("mongodb://localhost:27017/fmslog");
                mogoClient = new MongoClient(connection);
            }
            //mongodb = mogoClient.GetDatabase("efms");
            mongodb = mogoClient.GetDatabase(connection.Split("/")[3]);
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
