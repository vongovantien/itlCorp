using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public class Templates
    {
        public static string ExelImportEx = "ImportTemplate.xlsx";
        public struct Container
        {
            public static string ExelImportFileName = "Container";
        }
        public struct Goods
        {
            public static string ExelImportFileName = "Goods";
        }
        public struct SurCharge
        {
            public static string ExcelImportFileName = "LogisticsImportChargeTemplate.xlsx";
            public static string ExelImportSurchargeFileName = "Surcharge";
        }
    }
}
