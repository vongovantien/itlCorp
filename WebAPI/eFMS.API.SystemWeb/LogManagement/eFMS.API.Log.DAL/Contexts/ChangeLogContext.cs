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
        public IMongoCollection<CatCurrencyExchange> CatCurrencyExchanges
        {
            get
            {
                return _database.GetCollection<CatCurrencyExchange>("CatCurrencyExchange");
            }
        }
        public IMongoCollection<CatCharge> CatCatCharges
        {
            get
            {
                return _database.GetCollection<CatCharge>("CatCharge");
            }
        }
        public IMongoCollection<CatChargeDefaultAccount> CatChargeDefaultAccounts
        {
            get
            {
                return _database.GetCollection<CatChargeDefaultAccount>("CatChargeDefaultAccount");
            }
        }
        public IMongoCollection<CatCommodityGroup> CatCommodityGroups
        {
            get
            {
                return _database.GetCollection<CatCommodityGroup>("CatCommodityGroup");
            }
        }
        public IMongoCollection<CatCommodity> CatCommodities
        {
            get
            {
                return _database.GetCollection<CatCommodity>("CatCommodity");
            }
        }
        public IMongoCollection<CatCountry> CatCountries
        {
            get
            {
                return _database.GetCollection<CatCountry>("CatCountry");
            }
        }
        public IMongoCollection<CatPartner> CatPartners
        {
            get
            {
                return _database.GetCollection<CatPartner>("CatPartner");
            }
        }
        public IMongoCollection<CatPlace> CatPlaces
        {
            get
            {
                return _database.GetCollection<CatPlace>("CatPlace");
            }
        }
        public IMongoCollection<CatStage> CatStages
        {
            get
            {
                return _database.GetCollection<CatStage>("CatStage");
            }
        }
        public IMongoCollection<CatUnit> CatUnits
        {
            get
            {
                return _database.GetCollection<CatUnit>("CatUnit");
            }
        }
    }
}
