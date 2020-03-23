﻿namespace eFMS.API.ReportData.Models
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
            public const string UserUrl = "/System/api/v1/en-US/SysUser/Query";
        }

        public class Accounting
        {
            public const string AdvancePaymentUrl = "/Accounting/api/v1/en-US/AcctAdvancePayment/QueryData";

            public const string GetDataBravoSOAUrl = "/Accounting/api/v1/en-US/AcctSOA/GetDataExporttBravoFromSOA?soaNo=";

            public const string GetDataSOAOPSUrl = "/Accounting/api/v1/en-US/AcctSOA/GetDataExportSOAOPS?soaNo=";


            public const string SettlementPaymentUrl = "/Accounting/api/v1/en-US/AcctSettlementPayment/QueryData";
            public const string DetailAdvancePaymentExportUrl = "/Accounting/api/v1/en-US/AcctAdvancePayment/DetailAdvancePaymentExport";
            public const string DetailSettlementPaymentExportUrl = "/Accounting/api/v1/en-US/AcctSettlementPayment/DetailSettlementPaymentExport";
            public const string DetailSOAExportUrl = "/Accounting/api/v1/en-US/AcctSOA/GetDataExportSOABySOANo?soaNo=";
            public const string GetDataSOAAirfreightExportUrl = "/Accounting/api/v1/en-US/AcctSOA/GetDataExportAirFrieghtBySOANo?soaNo=";
            public const string GetGroupRequestsByAdvanceNoList = "/Accounting/api/v1/en-US/AcctAdvancePayment/GetGroupRequestsByAdvanceNoList";
            public const string QueryDataSettlementExport = "/Accounting/api/v1/en-US/AcctSettlementPayment/QueryDataSettlementExport";

        }

        public class Documentation
        {
            public const string HouseBillDetailUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/GetById?Id=";
            public const string NeutralHawbExportUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/NeutralHawbExport";
            public const string AirwayBillExportUrl = "/Documentation/api/v1/en-US/CsAirWayBill/AirwayBillExport?jobId=";
        }
    }
}
