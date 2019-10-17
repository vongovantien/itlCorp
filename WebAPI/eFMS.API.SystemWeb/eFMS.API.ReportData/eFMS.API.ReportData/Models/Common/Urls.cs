using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class Urls
    {
        public class Catelogue
        {
            public const string CountryUrl = "/Catalogue/api/v1/en-US/CatCountry/query";
            public const string CatplaceUrl = "/Catalogue/api/v1/en-US/CatPlace/query";
            public const string CatPartnerUrl = "/Catalogue/api/v1/en-US/CatPartner/query";
            public const string CatCommodityUrl = "/Catalogue/api/v1/en-US/CatCommonity/query";
            public const string CatCommodityGroupUrl = "/Catalogue/api/v1/en-US/CatCommodityGroup/query";
            public const string CatStageUrl = "/Catalogue/api/v1/en-US/CatStage/query";
            public const string CatUnitUrl = "/api/v1/en-US/CatUnit/query";
            public const string CatchargeUrl = "/Catalogue/api/v1/en-US/CatCharge/query";
            public const string CatCurrencyUrl = "/Catalogue/api/v1/en-US/CatCurrency/getAllByQuery";
        }
        public class CustomClearance
        {
            public const string CustomClearanceUrl = "/Operation/api/v1/en-US/CustomsDeclaration/Query";

        }
        public class System
        {
            public const string DepartmentUrl = "/System/api/v1/en-US/CatDepartment/QueryData";
            public const string OfficeUrl = "/System/api/v1/en-US/SysOffice/Query";
            public const string CompanyUrl = "/System/api/v1/en-US/SysCompany/Query";
            public const string GroupUrl = "/System/api/v1/en-US/SysGroup/Query";
        }
    }
}
