using eFMS.API.Catalogue.Service.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.Helpers
{
    public class MongoDbHelper
    {
        private static MongoClient mogoClient;
        private static IMongoDatabase mongodb;
        public static IMongoDatabase GetDatabase()
        {
            if(mogoClient == null)
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
        public static void Delete(string collectionName, ChangeLog model)
        {
            var filter = Builders<ChangeLog>.Filter.Where(e => model.Id == e.Id);
            var result = mongodb.GetCollection<ChangeLog>(collectionName).FindOneAndDelete(filter);
        }
    }
}
