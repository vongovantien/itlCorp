using System.IO;

namespace eFMS.API.Report.DL.Common
{
    public static class ReportConstants
    {
        //Report Name
        public const string Standard_Report = "Standard Report";
        public const string Shipment_Overview = "Shipment Overview";
        public const string Accountant_PL_Sheet = "Accountant P/L Sheet";
        public const string Job_Profit_Analysis = "Job Profit Analysis";
        public const string Summary_Of_Costs_Incurred = "Summary Of Costs Incurred";
        public const string Summary_Of_Revenue_Incurred = "Summary Of Revenue Incurred";

        public const string Export_Excel = "Export Excel";

        // Main Template folder
        public static string PathOfTemplateExcel = Path.Combine(Directory.GetCurrentDirectory(), @"FormatExcel\TemplateExport");

        // Template Name
        public const string AP_Account_Template = "AP_Account_Template.xlsx";
        public const string Edoc_Report_Template = "Template-Report-eDOC.xlsx";
        public const string AP_Standart_Report = "AP_Standart_Report.xlsx";
        public const string AR_DebitDetail_Template = "AR_DebitDetail_Template.xlsx";
        public const string AR_SUMMARY_TEMPLATE = "AR_SUMMARY_TEMPLATE.xlsx";
        public static string AR_SUMMARY_TEMPLATE_NO_ARGEEMENT = "AR_SUMMARY_TEMPLATE_NO_ARGEEMENT.xlsx";
        public const string Receipt_Advance_Report_Teamplate = "Receipt_Advance_Report _Teamplate.xlsx";
        public const string Statement_of_Receivable_Agency = "Statement_of_Receivable-Agency.xlsx";
        public const string Statement_of_Receivable_Customer = "Statement_of_Receivable-Customer.xlsx";
        public const string Settlement_Detail_Template = "Settlement-Detail Template.xlsx";
        public const string Settlement_General_Preview = "Settlement-General-Preview.xlsx";
        public const string Commission_OPS_VND = "Commission-OPS-VND.xlsx";
        public const string Commission_PR = "Commission-PR.xlsx";
        public const string Incentive = "Incentive.xlsx";
        public const string PHIEU_CAN_ACS_Template = "PHIEU-CAN-ACS-Template.xlsx";
        public const string PHIEU_CAN_NCTS_ALS_Template = "PHIEU-CAN-NCTS-ALS-Template.xlsx";
        public const string Shipment_Overview_FCL = "Shipment-Overview-FCL.xlsx";
        public const string ShipmentOverviewLCL = "ShipmentOverviewLCL.xlsx";
        public const string DebitAmountDetailByContract = "DebitAmountDetailByContract.xlsx";
        public const string CombineBillingByShipmentVND = "CombingBillingByJobVND.xlsx";
        public const string CombineBillingByShipmentUSD = "CombingBillingByJobUSD.xlsx";
        public const string OutsourcingRecognisingTemplate = "Outsourcing Recognising Template.xlsx";
        public const string Accounting_PL_Sheet = "AccountingPLSheet.xlsx";

        public static readonly string LG_SHIPMENT = "CL";// Custom Logistic
        public static readonly string SEF_SHIPMENT = "SEF"; //Sea FCL Export
        public static readonly string SIF_SHIPMENT = "SIF"; //Sea FCL Import
        public static readonly string SEL_SHIPMENT = "SEL"; //Sea LCL Export
        public static readonly string SIL_SHIPMENT = "SIL"; //Sea LCL Import
        public static readonly string SEC_SHIPMENT = "SEC"; //Sea Consol Export
        public static readonly string SIC_SHIPMENT = "SIC"; //Sea Consol Import
        public static readonly string AE_SHIPMENT = "AE"; //Sea Air Export
        public static readonly string AI_SHIPMENT = "AI"; //Sea Air Import
        public static readonly string IT_SHIPMENT = "IT"; //Inland Trucking


        public static readonly string SEF_HBL = "HBL";
        public static readonly string OPS_SHIPMENT = "LOG";
        public static readonly string CLEARANCE_FROM_EFMS = "eFMS";

        public static readonly string STATUS_APPROVAL_NEW = "New";
        public static readonly string STATUS_APPROVAL_DENIED = "Denied";
        public static readonly string STATUS_APPROVAL_DONE = "Done";
        public static readonly string STATUS_APPROVAL_LEADERAPPROVED = "LeaderApproved";
        public static readonly string STATUS_APPROVAL_DEPARTMENTAPPROVED = "DepartmentManagerApproved";
        public static readonly string STATUS_APPROVAL_ACCOUNTANTAPPRVOVED = "AccountantManagerApproved";

        public static readonly string CHARGE_BUY_TYPE = "BUY";
        public static readonly string CHARGE_SELL_TYPE = "SELL";
        public static readonly string CHARGE_OBH_TYPE = "OBH";

        #region -- INFO COMPANY --
        public static readonly string COMPANY_NAME = "INDO TRANS LOGISTICS CORPORATION";
        public static readonly string COMPANY_ADDRESS1 = "52‎-‎54‎-‎56 ‎Truong Son St‎.‎, ‎Tan Binh Dist‎.‎, ‎HCM City‎, ‎Vietnam‎";
        public static readonly string COMPANY_ADDRESS2 = "";
        public static readonly string COMPANY_WEBSITE = "www‎.‎itlvn‎.‎com‎";
        public static readonly string COMPANY_CONTACT = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
        public static readonly string COMPANY_TAXCODE = "0301909173";
        public static readonly string COMPANY_NAME_LOCAL = "Công ty CP Giao Nhận và Vận Chuyển Indo Trần";
        public static readonly string COMPANY_ADDRESS_LOCAL = "52‎-‎54‎-‎56 ‎Trường Sơn, Quận Tân Bình, TP. HCM‎";
        #endregion
        public static readonly string CURRENCY_LOCAL = "VND";
        public static readonly string CURRENCY_USD = "USD";
        public static readonly string CURRENCY_ORIGIN = "ORIGIN";

        public const string HBLOFLANDING_ITL = "ITL";
        public const string HBLOFLANDING_ITL_FRAME = "ITL_FRAME";
        public const string HBLOFLANDING_FBL_FRAME = "FBL_FRAME";
        public const string HBLOFLANDING_FBL_NOFRAME = "FBL_NOFRAME";
        public const string HBLOFLANDING_ITL_KESCO = "ITL_KESCO";
        public const string HBLOFLANDING_ITL_FRAME_KESCO = "ITL_FRAME_KESCO";
        public const string HBLOFLANDING_ITL_SEKO = "ITL_SEKO";
        public const string HBLOFLANDING_ITL_FRAME_SAMKIP = "ITL_FRAME_SAMKIP";

        public const string HOUSEAIRWAYBILLLASTEST_ITL = "LASTEST_ITL";
        public const string HOUSEAIRWAYBILLLASTEST_ITL_FRAME = "LASTEST_ITL_FRAME";
        public const string HOUSEAIRWAYBILLLASTEST_HAWB = "LASTEST_HAWB";
        public const string HOUSEAIRWAYBILLLASTEST_HAWB_FRAME = "HAWB_FRAME";

        public const string CODE_ITL = "ITL7939";
        public const string NO_HOUSE = "N/H";

        public static readonly string CURRENT_STATUS_CANCELED = "Canceled";


        public const string PERMISSION_RANGE_ALL = "All";
        public const string PERMISSION_RANGE_OWNER = "Owner";
        public const string PERMISSION_RANGE_GROUP = "Group";
        public const string PERMISSION_RANGE_DEPARTMENT = "Department";
        public const string PERMISSION_RANGE_OFFICE = "Office";
        public const string PERMISSION_RANGE_COMPANY = "Company";

        public static readonly string PARTNER_LOCATION_DOMESTIC = "Domestic";
        public static readonly string PARTNER_LOCATION_OVERSEA = "Oversea";

        public const string Crystal_Preview = "Crystal Preview";
        //Report Name
        public const string Monthly_Sale_Report = "Monthly Sale Report";
        public const string Sale_Report_By_Quater = "Sale Report By Quater";
        public const string Sale_Report_By_Department = "Sale Report By Department";
        public const string Summary_Sale_Report = "Summary Sale Report";
        public const string Combination_Statistic_Report = "Combination Statistic Report";
        public const string Sale_KickBack_Report = "Sale KickBack Report";

        #region -- AR CREDIT TYPE --
        public static readonly string CREDIT_NOTE_TYPE_CODE = "CREDITNOTE";
        public static readonly string CREDIT_SOA_TYPE_CODE = "CREDITSOA";
        #endregion
    }
}
