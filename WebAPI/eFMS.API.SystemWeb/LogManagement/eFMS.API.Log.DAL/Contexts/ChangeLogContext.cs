using eFMS.API.Log.Service.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Contexts
{
    public class ChangeLogContext
    {
        private readonly IMongoDatabase _database = null;

        public ChangeLogContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }
        public IMongoCollection<CatCurrency> CatCurrencies
        {
            get
            {
                return _database.GetCollection<CatCurrency>("CatCurrency");
            }
        }
    }
}
