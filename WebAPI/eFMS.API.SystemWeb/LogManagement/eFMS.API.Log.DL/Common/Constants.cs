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
        CatUnit = 11
    }
}
