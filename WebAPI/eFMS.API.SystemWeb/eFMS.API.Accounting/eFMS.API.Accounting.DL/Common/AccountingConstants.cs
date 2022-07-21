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
        public static readonly string STATUS_APPROVAL_LEADERAPPROVED = "Leader Approved";
        public static readonly string STATUS_APPROVAL_DEPARTMENTAPPROVED = "Department Manager Approved";
        public static readonly string STATUS_APPROVAL_ACCOUNTANTAPPRVOVED = "Accountant Manager Approved";
        public static readonly string STATUS_APPROVAL_REQUESTAPPROVAL = "Request Approval";
        #endregion -- STATUS APPROVAL --

        #region -- TYPE CHARGE --
        public static readonly string TYPE_CHARGE_BUY = "BUY";
        public static readonly string TYPE_CHARGE_OTHER = "OTHER";
        public static readonly string TYPE_CHARGE_SELL = "SELL";
        public static readonly string TYPE_CHARGE_OBH = "OBH";
        public static readonly string TYPE_CHARGE_OBH_BUY = "OBH-BUY";
        public static readonly string TYPE_CHARGE_OBH_SELL = "OBH-SELL";
        #endregion -- TYPE CHARGE --

        #region -- TYPE CHARGE DEFAULT --
        public static readonly string CHARGE_DEFAULT_CONG_NO = @"Công Nợ";
        #endregion

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
        public static readonly string DEPT_CODE_ACCOUNTANT = "FA-01";
        public static readonly string DEPT_CODE_OPS = "LOGISTIC-01";
        #endregion -- DEPARTMENT CODE --

        #region -- INFO COMPANY --
        public static readonly string COMPANY_NAME = "INDO TRANS LOGISTICS CORPORATION";
        public static readonly string COMPANY_ADDRESS1 = "52‎-‎54‎-‎56 ‎Truong Son St‎.‎, ‎Tan Binh Dist‎.‎, ‎HCM City‎, ‎Vietnam‎";
        public static readonly string COMPANY_ADDRESS2 = "";
        public static readonly string COMPANY_WEBSITE = "www‎.‎itlvn‎.‎com‎";
        public static readonly string COMPANY_CONTACT = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
        public static readonly string OFFICE_CODE_HCM = "ITLHCM";
        public static readonly string OFFICE_CODE_DAD = "ITLDAD";
        public static readonly string OFFICE_CODE_HAN = "ITLHAN";
        #endregion -- INFO COMPANY --

        #region -- PAYMENT METHOD --
        public static readonly string PAYMENT_METHOD_CASH = "Cash";
        public static readonly string PAYMENT_METHOD_BANK = "Bank";
        public static readonly string PAYMENT_METHOD_BANK_TRANSFER = "Bank Transfer";
        public static readonly string PAYMENT_METHOD_OTHER = "Other";
        public static readonly string PAYMENT_METHOD_NETOFF_SHPT = "NETOFF_SHPT";
        public static readonly string PAYMENT_METHOD_CLEAR_ADVANCE = "Clear-Advance";
        public static readonly string PAYMENT_METHOD_CLEAR_ADVANCE_CASH = "Clear-Advance-Cash";
        public static readonly string PAYMENT_METHOD_CLEAR_ADVANCE_BANK = "Clear-Advance-Bank";
        public static readonly string PAYMENT_METHOD_COLL_INTERNAL = "COLL - Internal";
        public static readonly string PAYMENT_METHOD_MANAGEMENT_FEE = "Management Fee";
        public static readonly string PAYMENT_METHOD_OTHER_FEE = "Other Fee";

        #endregion -- PAYMENT METHOD --

        #region -- STATUS SOA --
        public static readonly string STATUS_SOA_NEW = "New";
        public static readonly string STATUS_SOA_ISSUED_INVOICE = "Issued Invoice";
        public static readonly string STATUS_SOA_ISSUED_VOUCHER = "Issued Voucher";
        #endregion -- STATUS SOA --

        public static readonly string CURRENCY_LOCAL = "VND";
        public static readonly string CURRENCY_USD = "USD";
        public static readonly short SpecialGroup = 11;
        public static readonly string PositionManager = "Manager-Leader";
        public static readonly string DeptTypeAccountant = "ACCOUNTANT";
        #region -- CHARGE
        public static readonly string CHARGE_AIR_FREIGHT_CODE = "SA_A_F_Air";
        public static readonly string CHARGE_BA_AIR_FREIGHT_CODE = "BA_A_F_Air";
        public static readonly string CHARGE_AIR_FREIGHT = "Air freight";

        public static readonly string CHARGE_FUEL_SURCHARGE_CODE = "SA_FSC_Air";
        public static readonly string CHARGE_BA_FUEL_SURCHARGE_CODE = "BA_FSC_Air";
        public static readonly string CHARGE_FUEL_SURCHARGE = "Fuel Surcharge";

        public static readonly string CHARGE_WAR_RISK_SURCHARGE_CODE = "SA_WRS_Air";
        public static readonly string CHARGE_BA_WAR_RISK_SURCHARGE_CODE = "BA_WRS_Air";
        public static readonly string CHARGE_WAR_RISK_SURCHARGE = "War risk Surcharge";

        public static readonly string CHARGE_SCREENING_CODE = "SA_SCR_Air";
        public static readonly string CHARGE_BA_SCREENING_CODE = "BA_SCR_Air";
        public static readonly string CHARGE_SCREENING_FEE = "Screening fee";
        public static readonly string CHARGE_X_RAY = "X-Ray Charge";

        public static readonly string CHARGE_AWB_FEE = "Air Waybill fee";
        public static readonly string CHARGE_AWB = "Air Waybill";
        public static readonly string CHARGE_AWB_FEE_CODE = "SA_AWB_AIR";
        public static readonly string CHARGE_BA_AWB_FEE_CODE = "BA_AWB_Air";

        public static readonly string CHARGE_SA_DAN_AIR_CODE = "SA_DAN_AIR";
        public static readonly string CHARGE_BA_DAN_AIR_CODE = "BA_DAN_Air";
        public static readonly string CHARGE_SA_DAN_AIR_FEE = "Dangerous Fee";

        public static readonly string CHARGE_AMS_FEE = "Automated Manifest System";
        public static readonly string CHARGE_AMS_FEE_CODE = "SA_AMS_AIR";
        public static readonly string CHARGE_BA_AMS_FEE_CODE = "BA_AMS_Air";

        public static readonly string CHARGE_SA_OTH_AIR_CODE = "SA_OTH_AIR";
        public static readonly string CHARGE_BA_OTH_AIR_CODE = "BA_OTH_Air";
        public static readonly string CHARGE_SA_OTH_FEE = "Other Charges";

        public static readonly string CHARGE_BA_DHL_AIR_CODE = "BA_HDL_Air";
        public static readonly string CHARGE_HANDLING_FEE = "handling";
        public static readonly string CHARGE_SA_HDL_AIR_CODE = "SA_HDL_Air";
        #region -- TYPE SOA--
        public static readonly string TYPE_SOA_CREDIT = "Credit";
        public static readonly string TYPE_SOA_DEBIT = "Debit";
        public static readonly string TYPE_SOA_OBH = "Obh";




        #endregion






        #endregion

        #region ACOUNTING MANAGEMENT
        public static readonly string ACCOUNTING_VOUCHER_TYPE = "Voucher";
        public static readonly string ACCOUNTING_INVOICE_TYPE = "Invoice";
        public static readonly string ACCOUNTING_INVOICE_TEMP_TYPE = "InvoiceTemp";
        public static readonly string ACCOUNTING_INVOICE_STATUS_NEW = "New";
        public static readonly string ACCOUNTING_INVOICE_STATUS_UPDATED = "Updated Invoice";
        public static readonly string ACCOUNTING_PAYMENT_STATUS_UNPAID = "Unpaid";
        public static readonly string ACCOUNTING_PAYMENT_STATUS_PAID = "Paid";
        public static readonly string ACCOUNTING_PAYMENT_STATUS_PAID_A_PART = "Paid A Part";
        public static readonly string ACCOUNTING_PAYMENT_TYPE_NORMAL = "Normal";
        public static readonly string ACCOUNTING_PAYMENT_TYPE_NET_OFF = "Net Off";



        #endregion

        public static readonly string ROLE_NONE = "None";
        public static readonly string ROLE_APPROVAL = "Approval";
        public static readonly string ROLE_AUTO = "Auto";
        public static readonly string ROLE_SPECIAL = "Special";

        public static readonly string LEVEL_LEADER = "Leader";
        public static readonly string LEVEL_MANAGER = "Manager";
        public static readonly string LEVEL_ACCOUNTANT = "Accountant";
        public static readonly string LEVEL_BOD = "BOD";

        public static readonly string ARGEEMENT_TYPE_TRIAL = "Trial";
        public static readonly string ARGEEMENT_TYPE_OFFICIAL = "Official";
        public static readonly string ARGEEMENT_TYPE_GUARANTEED = "Guaranteed";
        public static readonly string ARGEEMENT_TYPE_CASH = "Cash";
        public static readonly string ARGEEMENT_TYPE_PARENT= "Parent Contract";

        public static readonly string STATUS_ACTIVE = "Active";
        public static readonly string STATUS_INACTIVE = "Inactive";

        public static readonly string STATUS_SYNCED = "Synced";
        public static readonly string STATUS_REJECTED = "Rejected";

        public static readonly string ACCOUNTANT_TYPE_DEBIT = "DEBIT";
        public static readonly string ACCOUNTANT_TYPE_CREDIT = "CREDIT";
        public static readonly string ACCOUNTANT_TYPE_INVOICE = "INVOICE";

        public static readonly string AGREEMENT_BASE_ON_INVOICE_DATE = "Invoice Date";
        public static readonly string AGREEMENT_BASE_ON_CONFIRMED_BILLING = "Confirmed Billing";

        public static readonly string RECEIPT_STATUS_DRAFT = "Draft";
        public static readonly string RECEIPT_STATUS_DONE = "Done";
        public static readonly string RECEIPT_STATUS_CANCEL = "Cancel";

        public static readonly string SETTLEMENT_TYPE_DIRECT = "DIRECT";
        public static readonly string SETTLEMENT_TYPE_EXISTING = "EXISTING";

        public static readonly string RECEIPT_CLASS_CLEAR_DEBIT = "Clear Debit";
        public static readonly string RECEIPT_CLASS_ADVANCE = "Advance";
        public static readonly string RECEIPT_CLASS_COLLECT_OBH = "Collect OBH";
        public static readonly string RECEIPT_CLASS_PAY_OBH = "Pay OBH";
        public static readonly string RECEIPT_CLASS_NET_OFF = "Net Off";

        #region PAYMENT
        public static readonly string PAYMENT_TYPE_CODE_COLLECT_OBH = "COLL_OBH";
        public static readonly string PAYMENT_TYPE_CODE_COLLECT_OTHER = "COLL_OTH";
        public static readonly string PAYMENT_TYPE_CODE_PAY_OBH = "PAY_OBH";
        public static readonly string PAYMENT_TYPE_CODE_PAY_OTHER = "PAY_OTH";
        public static readonly string PAYMENT_TYPE_CODE_ADVANCE = "ADV";

        public static readonly string PAYMENT_TYPE_NAME_COLLECT_OBH = "COLLECT-OBH";
        public static readonly string PAYMENT_TYPE_NAME_COLLECT_OTHER = "COLLECT-OTHER";
        public static readonly string PAYMENT_TYPE_NAME_PAY_OBH = "PAY-OBH";
        public static readonly string PAYMENT_TYPE_NAME_PAY_OTHER = "PAY-OTHER";
        public static readonly string PAYMENT_TYPE_NAME_NET_OFF = "NETOFF";
        public static readonly string PAYMENT_TYPE_NAME_ADVANCE = "ADV";
        #endregion

        #region -- AR CREDIT TYPE --
        public static readonly string CREDIT_NOTE_TYPE_CODE = "CREDITNOTE";
        public static readonly string CREDIT_SOA_TYPE_CODE = "CREDITSOA";
        #endregion

        public static readonly string EMAIL_SETTING_AR_ALERT = "AR - Alert";

        public static readonly int MAX_CREDIT_LIMIT_RATE_CONTRACT = 120;

        public static readonly string ITL_BOD = "ITL.BOD";
        #region PAYABLE
        public static readonly string TRANSACTION_TYPE_PAYABLE_CREDIT = "CREDIT";
        public static readonly string TRANSACTION_TYPE_PAYABLE_OBH = "OBH";
        public static readonly string TRANSACTION_TYPE_PAYABLE_ADV = "ADV";
        public static readonly string TRANSACTION_TYPE_PAYABLE_COMBINE = "COMBINE";
        #endregion

        public const string OFFICE_HM = "ITLHM";
        public const string OFFICE_BH = "ITLHBH";

        public const string LINK_TYPE_AUTO_RATE = "AUTO_RATE";
        public const string TYPE_LINK_SOURCE_SHIPMENT = "Replicate";
        public const string OFFICE_TYPE_OUTSOURCE = "OutSource";
    }
}
