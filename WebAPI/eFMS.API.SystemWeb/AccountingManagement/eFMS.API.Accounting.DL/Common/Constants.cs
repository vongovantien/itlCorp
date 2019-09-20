using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Common
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

        public static readonly string TYPE_CHARGE_BUY = "BUY";
        public static readonly string TYPE_CHARGE_SELL = "SELL";
        public static readonly string TYPE_CHARGE_OBH = "OBH";

        public static readonly string STATUS_PAYMENT_NOTSETTLED = "NotSettled";
        public static readonly string STATUS_PAYMENT_SETTLED = "Settled";
        public static readonly string STATUS_PAYMENT_PARTIALSETTLEMENT = "PartialSettlement";

        public static readonly string CURRENT_STATUS_CANCELED = "Canceled";

        public static readonly string ADVANCE_TYPE_NORM = "Norm";
        public static readonly string ADVANCE_TYPE_INVOICE = "Invoice";
        public static readonly string ADVANCE_TYPE_OTHER = "Other";

    }
}
