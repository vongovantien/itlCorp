using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.DL.Common
{
    public class Constants
    {
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Active = "Active";
    }
    public class CatPlaceConstant
    {
        public const string Warehouse = "Warehouse";
        public const string Port = "Port";
        public const string Province = "Province"; 
        public const string District = "District";
        public const string Ward = "Ward";
    }
    public enum CategoryTable
    {
        CatCharge = 1,
        CatChargeDefaultAccount = 2,
        CatCommonityGroup = 3,
        CatCommodity = 4,
        CatCountry = 5,
        CatCurrency = 6,
        CatCurrencyExchange = 7,
        CatPartner = 8,
        CatPlace = 9,
        CatStage = 10,
        CatUnit = 11,
        Warehouse = 12,
        PortIndex = 13,
        Province = 14,
        District = 15,
        Ward = 16,
        ExchangeRate = 17
    }
}
