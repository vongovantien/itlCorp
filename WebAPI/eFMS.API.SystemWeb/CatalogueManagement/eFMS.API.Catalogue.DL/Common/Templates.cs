using System;
using System.Collections.Generic;
using System.Text;

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
        public struct CatCountry
        {
            public struct NameCaching
            {
                public static string ListName = "Countries";
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
        public struct CatUnit
        {
            public struct NameCaching
            {
                public static string ListName = "Units";
            }
        }
    }
}
