﻿
namespace eFMS.API.Documentation.DL.Common

{
    public static class DocumentConstants
    {
        //Stage from constants
        public const string FROM_USER = "User";
        public const string FROM_SYSTEM = "System";

        //Document stage code
        public static readonly string UPDATE_ATA_CODE = "U_ATA";
        public static readonly string UPDATE_ATD_CODE = "U_ATD";
        public static readonly string UPDATE_ICT_CODE = "U_ICT";
        public static readonly string UPDATE_POD_CODE = "U_POD";

        public static readonly string SEND_POD_CODE = "S_POD";
        public static readonly string SEND_PA_CODE = "S_PA";
        public static readonly string SEND_AN_CODE = "S_AN";
        public static readonly string SEND_DO_CODE = "S_DO";
        public static readonly string SEND_AL_CODE = "S_AL";
        public static readonly string SEND_HB_CODE = "S_HB";


        //tracking constants
        public const string IN_TRANSIT = "IN TRANSIT";
        public const string DONE = "DONE";

        //stage constants
        public const string UPDATE_ATA = "UPDATE_ATA";
        public const string UPDATE_ATD = "UPDATE_ATD";
        public const string UPDATE_INCOTERM = "UPDATE_INCOTERM";
        public const string UPDATE_POD = "UPDATE_POD";

        public const string SEND_POD = "SEND_POD";
        public const string SEND_PA = "SEND_PA";
        public const string SEND_AL = "SEND_AL";
        public const string SEND_AN = "SEND_AN";
        public const string SEND_DO = "SEND_DO";
        public const string SEND_HB = "SEND_HB";

        //container constants
        public static readonly string NO_CONTAINER = "No cont";

        public static readonly string LG_SHIPMENT = "CL";// Custom Logistic
        public static readonly string SEF_SHIPMENT = "SEF"; //Sea FCL Export
        public static readonly string SIF_SHIPMENT = "SIF"; //Sea FCL Import
        public static readonly string SEL_SHIPMENT = "SEL"; //Sea LCL Export
        public static readonly string SIL_SHIPMENT = "SIL"; //Sea LCL Import
        public static readonly string SEC_SHIPMENT = "SEC"; //Sea Consol Export
        public static readonly string SIC_SHIPMENT = "SIC"; //Sea Consol Import
        public static readonly string AE_SHIPMENT = "AE"; //Sea Air Export
        public static readonly string AI_SHIPMENT = "AI"; //Sea Air Import
        public static readonly string TK_SHIPMENT = "TK"; //Inland Trucking


        public static readonly string SEF_HBL = "HBL";
        public static readonly string OPS_SHIPMENT = "LOG";
        public static readonly string CLEARANCE_FROM_EFMS = "eFMS";
        public static readonly string CLEARANCE_FROM_REPLICATE = "Replicate";
        public static readonly string NON_CARRIER_PARTNER_CODE = "NONCARRIER";

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
        public const string ITL_BOD = "ITL.BOD";
        public const string OFFICE_HM = "ITLHM";
        public const string OFFICE_BH = "ITLHBH";


        public static readonly string CURRENT_STATUS_CANCELED = "Canceled";
        public static readonly string CURRENT_STATUS_FINISH = "Finish";
        public static readonly string SHIPMENT_TYPE_NOMINATED = "Nominated";
        public static readonly string SHIPMENT_TYPE_FREEHAND = "Freehand";

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

        public static readonly string LINK_CHARGE_TYPE_AUTO_RATE = "AUTO_RATE";
        public static readonly string LINK_CHARGE_TYPE_LINK_FEE = "LINK_FEE";

        public static readonly string ACCOUNTING_PAYMENT_STATUS_UNPAID = "Unpaid";
        public static readonly string ACCOUNTING_PAYMENT_STATUS_PAID = "Paid";
        public static readonly string ACCOUNTING_PAYMENT_STATUS_PAID_A_PART = "Paid A Part";

        public static readonly string CDNOTE_TYPE_DEBIT = "DEBIT";
        public static readonly string CDNOTE_TYPE_CREDIT = "CREDIT";
        public static readonly string CDNOTE_TYPE_INVOICE = "INVOICE";
        public static readonly string SOA_TYPE_DEBIT = "DEBIT";
        public static readonly string SOA_TYPE_CREDIT = "CREDIT";

        public static readonly string USER_EFMS_SYSTEM = "d1bb21ea-249a-455c-a981-dcb554c3b848";
        public static readonly string SETTING_FLOW_APPLY_TYPE_CHECK_POINT = "Check Point";
        public static readonly string SETTING_FLOW_APPLY_TYPE_NONE = "None";
        public static readonly string SETTING_FLOW_APPLY_TYPE_ALERT = "Alert";

        public static readonly string SETTING_FLOW_APPLY_PARTNER_TYPE_BOTH = "Both";
        public static readonly string SETTING_FLOW_APPLY_CONTRACT_TYPE_CASH = "Cash";
        public static readonly string SETTING_FLOW_APPLY_CONTRACT_TYPE_OFFICIAL = "Official";
        public static readonly string SETTING_FLOW_APPLY_CONTRACT_TYPE_TRIAL = "Trial";

        public static readonly string PARTNER_TYPE_CUSTOMER = "Customer";
        public static readonly string PARTNER_TYPE_AGENT = "Agent";
    }
}
