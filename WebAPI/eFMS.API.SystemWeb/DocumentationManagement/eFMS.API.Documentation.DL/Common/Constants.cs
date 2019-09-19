using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Common
{
    public static class Constants
    {
        public static readonly string SEF_SHIPMENT = "SEF";
        public static readonly string SEF_HBL = "HBL";
        public static readonly string OPS_SHIPMENT = "LOG";
        public static readonly string CLEARANCE_FROM_EFMS = "eFMS";

        public static readonly string STATUS_APPROVAL_NEW = "New";
        public static readonly string STATUS_APPROVAL_DENIED = "Denied";
        public static readonly string STATUS_APPROVAL_DONE = "Done";
        public static readonly string STATUS_APPROVAL_LEADERAPPROVED = "LeaderApproved";
        public static readonly string STATUS_APPROVAL_DEPARTMENTAPPROVED = "DepartmentManagerApproved";
        public static readonly string STATUS_APPROVAL_ACCOUNTANTAPPRVOVED = "AccountantManagerApproved";
    }
}
