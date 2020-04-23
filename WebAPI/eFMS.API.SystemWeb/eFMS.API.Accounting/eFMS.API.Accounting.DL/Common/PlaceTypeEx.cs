﻿using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Common
{
    public static class PlaceTypeEx
    {
        public static string GetPlaceType(CatPlaceTypeEnum placeType)
        {
            string result = "";
            switch (placeType)
            {
                case CatPlaceTypeEnum.BorderGate:
                    result = "BorderGate";
                    break;
                case CatPlaceTypeEnum.Branch:
                    result = "Branch";
                    break;
                case CatPlaceTypeEnum.Depot:
                    result = "Depot";
                    break;
                case CatPlaceTypeEnum.District:
                    result = "District";
                    break;
                case CatPlaceTypeEnum.Hub:
                    result = "Hub";
                    break;
                case CatPlaceTypeEnum.IndustrialZone:
                    result = "IndustrialZone";
                    break;
                case CatPlaceTypeEnum.Other:
                    result = "Other";
                    break;
                case CatPlaceTypeEnum.Port:
                    result = "Port";
                    break;
                case CatPlaceTypeEnum.Province:
                    result = "Province";
                    break;
                case CatPlaceTypeEnum.Station:
                    result = "Station";
                    break;
                case CatPlaceTypeEnum.Ward:
                    result = "Ward";
                    break;
                case CatPlaceTypeEnum.Warehouse:
                    result = "Warehouse";
                    break;
                default:
                    break;
            }
            return result;
        }
        public static string GetPartnerGroup(CatPartnerGroupEnum partnerGroup)
        {
            string result = "";
            switch (partnerGroup)
            {
                case CatPartnerGroupEnum.AGENT:
                    result = "AGENT";
                    break;
                case CatPartnerGroupEnum.CONSIGNEE:
                    result = "CONSIGNEE";
                    break;
                case CatPartnerGroupEnum.CUSTOMER:
                    result = "CUSTOMER";
                    break;
                case CatPartnerGroupEnum.PAYMENTOBJECT:
                    result = "PAYMENTOBJECT";
                    break;
                case CatPartnerGroupEnum.PETROLSTATION:
                    result = "PETROLSTATION";
                    break;
                case CatPartnerGroupEnum.SHIPPER:
                    result = "SHIPPER";
                    break;
                case CatPartnerGroupEnum.SHIPPINGLINE:
                    result = "SHIPPINGLINE";
                    break;
                case CatPartnerGroupEnum.SUPPLIER:
                    result = "SUPPLIER";
                    break;
                case CatPartnerGroupEnum.SUPPLIERMATERIAL:
                    result = "SUPPLIERMATERIAL";
                    break;
                case CatPartnerGroupEnum.CARRIER:
                    result = "CARRIER";
                    break;
                case CatPartnerGroupEnum.AIRSHIPSUP:
                    result = "AIRSHIPSUP";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
