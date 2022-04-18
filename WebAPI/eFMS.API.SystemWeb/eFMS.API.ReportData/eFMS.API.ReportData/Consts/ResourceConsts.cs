using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Consts
{
    /// <summary>Supports all classes in the .NET Framework class hierarchy and provides low-level services to derived classes. This is the ultimate base class of all classes in the .NET Framework; it is the root of the type hierarchy.</summary>
    public static class ResourceConsts
    {
        public const string GROUP_NAME = "GROUP INFORMATION";
        public const string Export_Excel = "Export Excel";

        //Report Name
        public const string Standard_Report = "Standard Report";
        public const string Shipment_Overview = "Shipment Overview";
        public const string Accountant_PL_Sheet = "Accountant P/L Sheet";
        public const string Job_Profit_Analysis = "Job Profit Analysis";
        public const string Summary_Of_Costs_Incurred = "Summary Of Costs Incurred";
        public const string Summary_Of_Revenue_Incurred = "Summary Of Revenue Incurred";

        // Main Template folder
        public static string PathOfTemplateExcel = Path.Combine(Directory.GetCurrentDirectory(), @"FormatExcel\TemplateExport");
        // Folder contain Settlement Template
        public const string SettlementPath = "SettlePayment";
        // Folder contain AR Module Template
        public const string AccountReceivablePath = "AccountReceivable";
        // Folder contain AP Module Template
        public const string AccountPayablePath = "AccountPayable";
        // Folder upload file to aws
        public const string FolderPreviewUploadFile = "Preview";

        // Template Name
        public const string AP_Account_Template = "AP_Account_Template.xlsx";
        public const string AP_Standart_Report = "AP_Standart_Report.xlsx";
        public const string AR_DebitDetail_Template = "AR_DebitDetail_Template.xlsx";
        public const string AR_SUMMARY_TEMPLATE = "AR_SUMMARY_TEMPLATE.xlsx";
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
    }
}
