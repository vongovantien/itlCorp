using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Common
{
    public static class Constants
    {
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Active = "Active";
        public const string FromEFMS = "eFMS";
        public const string FromEcus = "Ecus";
    }
    public static class ClearanceConstants
    {
        public const string Air_Service = "Air";
        public const string Sea_FCL_Service = "Sea FCL";
        public const string Sea_LCL_Service = "Sea LCL";
        public const string Trucking_Inland_Service = "Trucking Inland";
        public const string Rail_Service = "Rail";
        public const string Warehouse_Service = "Warehouse";
        public const string Air_Service_Type = "1";
        public const string Sea_FCL_Service_Type = "2";
        public const string Sea_LCL_Service_Type = "3";
        public const string Trucking_Inland_Service_Type = "4";
        public const string Rail_Service_Type = "5";
        public const string Warehouse_Service_Type6 = "6";
        public const string Warehouse_Service_Type9 = "9";

        public const string Import_Type = "N";
        public const string Export_Type = "X";
        public const string Import_Type_Value = "Import";
        public const string Export_Type_Value = "Export";

        public const string Route_Type_Do = "Do";
        public const string Route_Type_Xanh = "Xanh";
        public const string Route_Type_Vang = "Vang";
        public const string Route_Type_Red = "Red";
        public const string Route_Type_Green = "Green";
        public const string Route_Type_Yellow = "Yellow";
    }
    public static class CatPlaceConstant
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
