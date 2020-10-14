using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class DocumentConstants
    {
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
        public const string HBLOFLANDING_ITL_KESCO = "ITL_KESCO";
        public const string HBLOFLANDING_ITL_FRAME_KESCO = "ITL_FRAME_KESCO";
        public const string HBLOFLANDING_ITL_SEKO = "ITL_SEKO";
        public const string HBLOFLANDING_ITL_FRAME_SAMKIP = "ITL_FRAME_SAMKIP";

        public const string HOUSEAIRWAYBILLLASTEST_ITL = "LASTEST_ITL";
        public const string HOUSEAIRWAYBILLLASTEST_ITL_FRAME = "LASTEST_ITL_FRAME";
        public const string HOUSEAIRWAYBILLLASTEST_HAWB = "LASTEST_HAWB";
        public const string HOUSEAIRWAYBILLLASTEST_HAWB_FRAME = "HAWB_FRAME";

        public const string CODE_ITL = "ITL7939";

        public static readonly string CURRENT_STATUS_CANCELED = "Canceled";


        public const string PERMISSION_RANGE_ALL = "All";
        public const string PERMISSION_RANGE_OWNER = "Owner";
        public const string PERMISSION_RANGE_GROUP = "Group";
        public const string PERMISSION_RANGE_DEPARTMENT = "Department";
        public const string PERMISSION_RANGE_OFFICE = "Office";
        public const string PERMISSION_RANGE_COMPANY = "Company";
    }
}
