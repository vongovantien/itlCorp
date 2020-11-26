namespace eFMS.API.Catalogue.DL.Common
{
    public struct Templates
    {
        public static string ExcelImportEx = "ImportTemplate.xlsx";
        public struct CatArea
        {
            public struct NameCaching
            {
                public static string ListName = "Areas";
            }
        }
        public struct CatCharge
        {
            public static string ExcelImportFileName = "Charge";
            public struct NameCaching
            {
                public static string ListName = "CatCharge";
            }
        }

        public struct CatChartOfAccounts
        {
            public static string ExcelImportFileName = "ChartOfAccounts";
            public struct NameCaching
            {
                public static string ListName = "CatChartOfAccounts";
            }
        }

        public struct CatChargeDefaultAccount
        {
            public static string ExcelImportFileName = "VoucherTypeAccount";

            public struct NameCaching
            {
                public static string ListName = "ChargeDefaultAccounts";
            }
        }
        public struct CatCommodity
        {
            public static string ExcelImportFileName = "Commodity";
            public struct NameCaching
            {
                public static string ListName = "commodities";
            }
        }

        public struct CatCommodityGroup
        {
            public static string ExcelImportFileName = "CommodityGroup";
            public struct NameCaching
            {
                public static string ListName = "commoditiesgroup";
            }
        }

        public struct CatCountry
        {
            public static string ExcelImportFileName = "Country";
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
            public static string ExcelImportFileName = "Partner";
            public static string ExelImportCommercialCustomerFileName = "CommercialCustomer";
            public struct NameCaching
            {
                public static string ListName = "Partners";
            }
        }

        public struct CatContract
        {
            public static string ExcelImportFileName = "CustomerContract";
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
            public static string ExcelImportFileName = "Place";
            public struct NameCaching
            {
                public static string ListName = "Places";
            }
        }
        public struct CatStage
        {
            public static string ExcelImportFileName = "Stage";
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
