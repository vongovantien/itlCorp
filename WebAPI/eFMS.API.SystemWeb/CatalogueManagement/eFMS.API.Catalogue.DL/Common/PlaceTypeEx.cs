using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Common
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
    }
}
