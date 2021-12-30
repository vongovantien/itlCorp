namespace eFMS.API.ReportData.Models
{
    public class Urls
    {
        public class Catelogue
        {
            public const string CountryUrl = "/api/v1/en-US/CatCountry/query";
            public const string CatplaceUrl = "/api/v1/en-US/CatPlace/QueryExport";
            public const string CatPartnerUrl = "/api/v1/en-US/CatPartner/QueryExport";
            public const string CatAgreementUrl = "/api/v1/en-US/CatPartner/QueryExportAgreement";
            public const string CatCommodityUrl = "/api/v1/en-US/CatCommonity/query";
            public const string CatCommodityGroupUrl = "/api/v1/en-US/CatCommodityGroup/query";
            public const string CatStageUrl = "/api/v1/en-US/CatStage/query";
            public const string CatUnitUrl = "/api/v1/en-US/CatUnit/query";
            public const string CatchargeUrl = "/api/v1/en-US/CatCharge/QueryExport";
            public const string CatCurrencyUrl = "/api/v1/en-US/CatCurrency/getAllByQuery";
            public const string CatChartOfAccountsUrl = "/api/v1/en-US/CatChartOfAccounts/QueryExport";
            public const string CatIncotermListUrl = "/api/v1/en-US/CatIncoterm/QueryExport";
            public const string CatPotentialListUrl = "/api/v1/en-US/CatPotential/QueryExport";
            public const string CatBankUrl = "/api/v1/en-US/CatBank/getAllByQuery";

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

            public const string InvoicePaymentUrl = "/api/v1/en-US/AccountingPayment/ExportAccountingPayment";

            public const string CustomerPaymentUrl = "/api/v1/en-US/AccountingPayment/GetDataExportAccountingCustomerPayment";

            public const string GetDataBravoSOAUrl = "/api/v1/en-US/AcctSOA/GetDataExporttBravoFromSOA?soaNo=";

            public const string GetDataSOAOPSUrl = "/api/v1/en-US/AcctSOA/GetDataExportSOAOPS?soaNo=";


            public const string SettlementPaymentUrl = "/api/v1/en-US/AcctSettlementPayment/QueryData";
            public const string SettlementPaymentDetailListUrl = "/api/v1/en-US/AcctSettlementPayment/GetDataExportSettlementDetail";
            public const string DetailAdvancePaymentExportUrl = "/api/v1/en-US/AcctAdvancePayment/DetailAdvancePaymentExport";
            public const string DetailSettlementPaymentExportUrl = "/api/v1/en-US/AcctSettlementPayment/DetailSettlementPaymentExport";
            public const string GeneralSettlementPaymentExport = "/api/v1/en-US/AcctSettlementPayment/GeneralSettlementPaymentExport";
            public const string DetailSOAExportUrl = "/api/v1/en-US/AcctSOA/GetDataExportSOABySOANo?soaNo=";
            public const string GetDataSOAAirfreightExportUrl = "/api/v1/en-US/AcctSOA/GetDataExportAirFrieghtBySOANo?soaNo=";
            public const string GetDataSOASupplierAirfreightExportUrl = "/api/v1/en-US/AcctSOA/GetDataExportSOASupplierAirFrieghtBySOANo?soaNo=";
            public const string GetGroupRequestsByAdvanceNoList = "/api/v1/en-US/AcctAdvancePayment/GetGroupRequestsByAdvanceNoList";
            public const string QueryDataSettlementExport = "/api/v1/en-US/AcctSettlementPayment/QueryDataSettlementExport";
            public const string AccountingManagementExportUrl = "/api/v1/en-US/AccountingManagement/GetDataAcctMngtExport";
            public const string AccountingGetDataARSumaryExportUrl = "/api/v1/en-US/AccountReceivable/GetDataARSumaryExport";
            public const string AgencyPaymentUrl = "/api/v1/en-US/AccountingPayment/GetDataExportAccountingAgencyPayment";
            public const string GetDataExportReceiptAdvance = "/api/v1/en-US/AcctReceipt/GetDataExportReceiptAdvance";
            public const string GetDataCombineOpsUrl = "/api/v1/en-US/AcctCombineBilling/GetDataExportCombineOps?combineBillingNo=";
            public const string GetDataCombineOpsByPartnerUrl = "/api/v1/en-US/AcctCombineBilling/GetDataExportCombineOpsByPartner";
        }

        public class Documentation
        {
            public const string HouseBillDetailUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/GetById?Id=";
            public const string NeutralHawbExportUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/NeutralHawbExport";
            public const string AirwayBillExportUrl = "/Documentation/api/v1/en-US/CsAirWayBill/AirwayBillExport?jobId=";
            public const string GetDataShipmentOverviewUrl = "/Documentation/api/v1/en-US/Shipment/GetDataExportShipmentOverview";
            public const string GetDataShipmentOverviewFCLUrl = "/Documentation/api/v1/en-US/Shipment/GetDataExportShipmentOverviewFCL";
            public const string GetDataShipmentOverviewLCLUrl = "/Documentation/api/v1/en-US/Shipment/GetDataExportShipmentOverviewLCL";

            public const string GetDataSummaryOfCostsIncurredUrl = "/Documentation/api/v1/en-US/Shipment/GetDataSummaryOfCostsIncurred";
            public const string GetDataSummaryOfRevenueIncurredUrl = "/Documentation/api/v1/en-US/Shipment/GetDataSummaryOfRevenueIncurred";
            public const string GetDataSummaryOfCostsPartnerUrl = "/Documentation/api/v1/en-US/Shipment/GetDataCostsByPartner";

            public const string GetDataAccountingPLSheetUrl = "/Documentation/api/v1/en-US/Shipment/GetDataExportAccountingPlSheet";
            public const string GetDataStandardGeneralReportUrl = "/Documentation/api/v1/en-US/Shipment/QueryDataGeneralReport";
            public const string GetDataJobProfitAnalysisUrl = "/Documentation/api/v1/en-US/Shipment/GetDataJobProfitAnalysis";
            public const string GetDataHousebillDailyExportUrl = "/Documentation/api/v1/en-US/CsTransactionDetail/GetHousebillsDailyExport?issuedDate=";

            public const string GetDataCDNoteExportUrl = "/Documentation/api/v1/en-US/AcctCDNote/ExportOpsCdNote?jobId=";
            public const string GetDataCDNoteCombineExportUrl = "/Documentation/api/v1/en-US/AcctCDNote/ExportOpsCdNoteCombine";

            public const string GetDataCommissionPRReportUrl = "/Documentation/api/v1/en-US/Shipment/GetCommissionReport?userId=";
            public const string GetDataIncentiveReportUrl = "/Documentation/api/v1/en-US/Shipment/GetIncentiveReport?userId=";
            public const string AddReportLogUrl = "/Documentation/api/v1/en-US/ReportLog/AddNew";

            public const string GetDataExporDebCretInvUrl = "/Documentation/api/v1/en-US/AcctCDNote/GetDataAcctMngtDebCretInvExport";
        }

        public class Setting
        {
            public const string GetDataUnlockRequestExportUrl = "/api/v1/en-US/UnlockRequest/GetDataUnlockRequestsExport";
        }
    }
}
