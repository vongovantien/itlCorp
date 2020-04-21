﻿namespace eFMS.API.ReportData.Models
{
    public class Urls
    {
        public class Catelogue
        {
            public const string CountryUrl = "/api/v1/en-US/CatCountry/query";
            public const string CatplaceUrl = "/api/v1/en-US/CatPlace/QueryExport";
            public const string CatPartnerUrl = "/api/v1/en-US/CatPartner/QueryExport";
            public const string CatCommodityUrl = "/api/v1/en-US/CatCommonity/query";
            public const string CatCommodityGroupUrl = "/api/v1/en-US/CatCommodityGroup/query";
            public const string CatStageUrl = "/api/v1/en-US/CatStage/query";
            public const string CatUnitUrl = "/api/v1/en-US/CatUnit/query";
            public const string CatchargeUrl = "/api/v1/en-US/CatCharge/QueryExport";
            public const string CatCurrencyUrl = "/api/v1/en-US/CatCurrency/getAllByQuery";
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
            public const string AdvancePaymentUrl = "/api/v1/en-US/AcctAdvancePayment/QueryData";

            public const string GetDataBravoSOAUrl = "/api/v1/en-US/AcctSOA/GetDataExporttBravoFromSOA?soaNo=";

            public const string GetDataSOAOPSUrl = "/api/v1/en-US/AcctSOA/GetDataExportSOAOPS?soaNo=";


            public const string SettlementPaymentUrl = "/api/v1/en-US/AcctSettlementPayment/QueryData";
            public const string DetailAdvancePaymentExportUrl = "/api/v1/en-US/AcctAdvancePayment/DetailAdvancePaymentExport";
            public const string DetailSettlementPaymentExportUrl = "/api/v1/en-US/AcctSettlementPayment/DetailSettlementPaymentExport";
            public const string DetailSOAExportUrl = "/api/v1/en-US/AcctSOA/GetDataExportSOABySOANo?soaNo=";
            public const string GetDataSOAAirfreightExportUrl = "/api/v1/en-US/AcctSOA/GetDataExportAirFrieghtBySOANo?soaNo=";
            public const string GetGroupRequestsByAdvanceNoList = "/api/v1/en-US/AcctAdvancePayment/GetGroupRequestsByAdvanceNoList";
            public const string QueryDataSettlementExport = "/api/v1/en-US/AcctSettlementPayment/QueryDataSettlementExport";

        }

        public class Documentation
        {
            public const string HouseBillDetailUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/GetById?Id=";
            public const string NeutralHawbExportUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/NeutralHawbExport";
            public const string AirwayBillExportUrl = "/Documentation/api/v1/en-US/CsAirWayBill/AirwayBillExport?jobId=";
            public const string GetDataShipmentOverviewUrl = "/Documentation/api/v1/en-US/Shipment/GetDataExportShipmentOverview";
            public const string GetDataAccountingPLSheetUrl = "/Documentation/api/v1/en-US/Shipment/GetDataExportAccountingPlSheet";
        }
    }
}
