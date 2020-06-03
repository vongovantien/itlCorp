namespace eFMS.API.Accounting.DL.Common
{
    public static class AccountingConstants
    {
        public static readonly string SEF_SHIPMENT = "SEF";
        public static readonly string SEF_HBL = "HBL";
        public static readonly string OPS_SHIPMENT = "LOG";
        public static readonly string CLEARANCE_FROM_EFMS = "eFMS";

        #region -- STATUS APPROVAL --
        public static readonly string STATUS_APPROVAL_NEW = "New";
        public static readonly string STATUS_APPROVAL_DENIED = "Denied";
        public static readonly string STATUS_APPROVAL_DONE = "Done";
        public static readonly string STATUS_APPROVAL_LEADERAPPROVED = "LeaderApproved";
        public static readonly string STATUS_APPROVAL_DEPARTMENTAPPROVED = "DepartmentManagerApproved";
        public static readonly string STATUS_APPROVAL_ACCOUNTANTAPPRVOVED = "AccountantManagerApproved";
        public static readonly string STATUS_APPROVAL_REQUESTAPPROVAL = "RequestApproval";
        #endregion -- STATUS APPROVAL --

        #region -- TYPE CHARGE --
        public static readonly string TYPE_CHARGE_BUY = "BUY";
        public static readonly string TYPE_CHARGE_SELL = "SELL";
        public static readonly string TYPE_CHARGE_OBH = "OBH";
        public static readonly string TYPE_CHARGE_OBH_BUY = "OBH-BUY";
        public static readonly string TYPE_CHARGE_OBH_SELL = "OBH-SELL";
        #endregion -- TYPE CHARGE --

        #region -- STATUS PAYMENT --
        public static readonly string STATUS_PAYMENT_NOTSETTLED = "NotSettled";
        public static readonly string STATUS_PAYMENT_SETTLED = "Settled";
        public static readonly string STATUS_PAYMENT_PARTIALSETTLEMENT = "PartialSettlement";
        #endregion -- STATUS PAYMENT --

        public static readonly string CURRENT_STATUS_CANCELED = "Canceled";

        #region -- ADVANCE TYPE --
        public static readonly string ADVANCE_TYPE_NORM = "Norm";
        public static readonly string ADVANCE_TYPE_INVOICE = "Invoice";
        public static readonly string ADVANCE_TYPE_OTHER = "Other";
        #endregion -- ADVANCE TYPE --

        #region -- DEPARTMENT CODE --
        public static readonly string DEPT_CODE_ACCOUNTANT = "Accountant";
        public static readonly string DEPT_CODE_OPS = "OPS";
        #endregion -- DEPARTMENT CODE --

        #region -- INFO COMPANY --
        public static readonly string COMPANY_NAME = "INDO TRANS LOGISTICS CORPORATION";
        public static readonly string COMPANY_ADDRESS1 = "52‎-‎54‎-‎56 ‎Truong Son St‎.‎, ‎Tan Binh Dist‎.‎, ‎HCM City‎, ‎Vietnam‎";
        public static readonly string COMPANY_ADDRESS2 = "";
        public static readonly string COMPANY_WEBSITE = "www‎.‎itlvn‎.‎com‎";
        public static readonly string COMPANY_CONTACT = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
        #endregion -- INFO COMPANY --

        #region -- PAYMENT METHOD --
        public static readonly string PAYMENT_METHOD_CASH = "Cash";
        public static readonly string PAYMENT_METHOD_BANK = "Bank";
        #endregion -- PAYMENT METHOD --

        #region -- STATUS SOA --
        public static readonly string STATUS_SOA_NEW = "New";
        #endregion -- STATUS SOA --

        public static readonly string CURRENCY_LOCAL = "VND";
        public static readonly string CURRENCY_USD = "USD";
        public static readonly short SpecialGroup = 11;
        public static readonly string PositionManager = "Manager-Leader";
        public static readonly string DeptTypeAccountant = "ACCOUNTANT";
        #region -- CHARGE
        public static readonly string CHARGE_AIR_FREIGHT = "Air freight";
        public static readonly string CHARGE_FUEL_SURCHARGE = "Fuel Surcharge";
        public static readonly string CHARGE_WAR_RISK_SURCHARGE = "War risk Surcharge";
        public static readonly string CHARGE_SCREENING_FEE = "Screening fee";

        public static readonly string CHARGE_AWB_FEE = "Air Waybill fee";

        public static readonly string CHARGE_HANDLING_FEE = "Handling fee";
        #region -- TYPE SOA--
        public static readonly string TYPE_SOA_CREDIT = "Credit";
        public static readonly string TYPE_SOA_DEBIT = "Debit";



        #endregion






        #endregion

        public static readonly string TypeAcctManagement_Invoice = "Invoice";
        public static readonly string TypeAcctManagement_Voucher = "Voucher";

    }
}
