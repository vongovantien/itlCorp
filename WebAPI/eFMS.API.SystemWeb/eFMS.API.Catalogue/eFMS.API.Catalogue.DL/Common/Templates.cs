namespace eFMS.API.Catalogue.DL.Common
{
    public struct Templates
    {
        public static string ExelImportEx = "ImportTemplate.xlsx";
        public struct CatArea
        {
            public struct NameCaching
            {
                public static string ListName = "Areas";
            }
        }
        public struct CatCharge
        {
            public static string ExelImportFileName = "Charge";
            public struct NameCaching
            {
                public static string ListName = "CatCharge";
            }
        }
        public struct CatChargeDefaultAccount
        {
            public static string ExelImportFileName = "VoucherTypeAccount";

            public struct NameCaching
            {
                public static string ListName = "ChargeDefaultAccounts";
            }
        }
        public struct CatCommodity
        {
            public static string ExelImportFileName = "Commodity";
            public struct NameCaching
            {
                public static string ListName = "commodities";
            }
        }
        public struct CatCountry
        {
            public struct NameCaching
            {
                public static string ListName = "Countries";
            }
        }
        public struct CatCurrency
        {
            public struct NameCaching
            {
                public static string ListName = "currencies";
            }
        }
        public struct CatPartner
        {
            public static string ExelImportFileName = "Partner";
            public struct NameCaching
            {
                public static string ListName = "Partners";
            }
        }
        public struct CatSaleMan
        {
            public struct NameCaching
            {
                public static string ListName = "SalesMan";
            }
        }

        public struct SysBranch
        {
            public struct NameCaching
            {
                public static string ListName = "Branch";
            }
        }
        public struct CatPlace
        {
            public static string ExelImportFileName = "Place";
            public struct NameCaching
            {
                public static string ListName = "Places";
            }
        }
        public struct CatStage
        {
            public static string ExelImportFileName = "Stage";
            public struct NameCaching
            {
                public static string ListName = "Stages";
            }
        }
        public struct CatUnit
        {
            public struct NameCaching
            {
                public static string ListName = "Units";
            }
        }
    }
}
