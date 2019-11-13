using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class Constants
    {
        public static readonly string LG_SHIPMENT = "LG";// Custom Logistic
        public static readonly string SEF_SHIPMENT = "SEF"; //Sea FCL Export
        public static readonly string SIF_SHIPMENT = "SIF"; //Sea FCL Import
        public static readonly string SEL_SHIPMENT = "SEL"; //Sea LCL Export
        public static readonly string SIL_SHIPMENT = "SIL"; //Sea LCL Import
        public static readonly string SEC_SHIPMENT = "SEC"; //Sea Consol Export
        public static readonly string SIC_SHIPMENT = "SIC"; //Sea Consol Import
        public static readonly string AE_SHIPMENT = "AE"; //Sea Air Export
        public static readonly string AI_SHIPMENT = "AI"; //Sea Air Import
        //public static readonly string T_SHIPMENT = "T"; //Trucking
        

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
    }
}
