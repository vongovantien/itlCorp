using eFMS.API.Common;
using eFMS.API.Setting.Service.Models;
using eFMS.API.Setting.Service.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.Service.Contexts
{
    public class ChangeLogContext
    {
        private readonly IMongoDatabase _database;

        public ChangeLogContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.MongoConnection);
            if (client != null)
                _database = client.GetDatabase(settings.Value.MongoDatabase);
        }
        public IMongoCollection<CatCurrencyLog> CatCurrencies
        {
            get
            {
                return _database.GetCollection<CatCurrencyLog>("CatCurrency");
            }
        }
        public IMongoCollection<CatCurrencyExchangeLog> CatCurrencyExchanges
        {
            get
            {
                return _database.GetCollection<CatCurrencyExchangeLog>("CatCurrencyExchange");
            }
        }
        public IMongoCollection<CatChargeLog> CatCatCharges
        {
            get
            {
                return _database.GetCollection<CatChargeLog>("CatCharge");
            }
        }
        public IMongoCollection<CatChargeDefaultAccountLog> CatChargeDefaultAccounts
        {
            get
            {
                return _database.GetCollection<CatChargeDefaultAccountLog>("CatChargeDefaultAccount");
            }
        }
        public IMongoCollection<CatCommodityGroupLog> CatCommodityGroups
        {
            get
            {
                return _database.GetCollection<CatCommodityGroupLog>("CatCommodityGroup");
            }
        }
        public IMongoCollection<CatCommodityLog> CatCommodities
        {
            get
            {
                return _database.GetCollection<CatCommodityLog>("CatCommodity");
            }
        }
        public IMongoCollection<CatCountryLog> CatCountries
        {
            get
            {
                return _database.GetCollection<CatCountryLog>("CatCountry");
            }
        }
        public IMongoCollection<CatPartnerLog> CatPartners
        {
            get
            {
                return _database.GetCollection<CatPartnerLog>("CatPartner");
            }
        }
        public IMongoCollection<CatPlaceLog> CatPlaces
        {
            get
            {
                return _database.GetCollection<CatPlaceLog>("CatPlace");
            }
        }
        public IMongoCollection<CatStageLog> CatStages
        {
            get
            {
                return _database.GetCollection<CatStageLog>("CatStage");
            }
        }
        public IMongoCollection<CatUnitLog> CatUnits
        {
            get
            {
                return _database.GetCollection<CatUnitLog>("CatUnit");
            }
        }
    }
}
