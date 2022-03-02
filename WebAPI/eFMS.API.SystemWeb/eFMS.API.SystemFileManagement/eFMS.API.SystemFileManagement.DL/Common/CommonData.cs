﻿using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Common
{
    public class CommonData
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }


    public static class CustomData
    {
        //Define list services
        public static readonly List<CommonData> Services = new List<CommonData>
        {
            new CommonData { Value = "CL", DisplayName = "Custom Logistic" },
            new CommonData { Value = "AI", DisplayName = "Air Import" },
            new CommonData { Value = "AE", DisplayName = "Air Export" },
            new CommonData { Value = "SFE", DisplayName = "Sea FCL Export" },
            new CommonData { Value = "SLE", DisplayName = "Sea LCL Export" },
            new CommonData { Value = "SFI", DisplayName = "Sea FCL Import" },
            new CommonData { Value = "SLI", DisplayName = "Sea LCL Import" },
            new CommonData { Value = "SCE", DisplayName = "Sea Consol Export" },
            new CommonData { Value = "SCI", DisplayName = "Sea Consol Import" },
            new CommonData { Value = "IT", DisplayName = "Inland Trucking " }
        };

        //Define list status of SOA
        public static readonly List<CommonData> StatusSoa = new List<CommonData>
        {
            new CommonData { Value = "New", DisplayName = "New" },
            new CommonData { Value = "RequestConfirmed", DisplayName = "Request Confirmed" },
            new CommonData { Value = "Confirmed", DisplayName = "Confirmed" },
            new CommonData { Value = "NeedRevise", DisplayName = "Need Revise" },
            new CommonData { Value = "Done", DisplayName = "Done" }
        };


        public static readonly List<CommonData> PaymentMethod = new List<CommonData>
        {
            new CommonData { Value = "Cash", DisplayName = "Cash" },
            new CommonData { Value = "Bank", DisplayName = "Bank Transfer" },
            new CommonData { Value = "Other", DisplayName = "Other" },
        };

        //Define list status approve advance
        public static readonly List<CommonData> StatusApproveAdvance = new List<CommonData>
        {
            new CommonData { Value = "New", DisplayName = "New" },
            new CommonData { Value = "Request Approval", DisplayName = "Request Approval" },
            new CommonData { Value = "Leader Approved", DisplayName = "Leader Approved" },
            new CommonData { Value = "Department Manager Approved", DisplayName = "Department Manager Approved" },
            new CommonData { Value = "Accountant Manager Approved", DisplayName = "Accountant Manager Approved" },
            new CommonData { Value = "Done", DisplayName = "Done" },
            new CommonData { Value = "Denied", DisplayName = "Denied" }
        };

        // Defined list payment type = other with code and name
        public static readonly List<CommonData> PaymentTypeOther = new List<CommonData>
        {
            new CommonData { Value = AccountingConstants.PAYMENT_TYPE_CODE_PAY_OBH, DisplayName = AccountingConstants.PAYMENT_TYPE_NAME_PAY_OBH },
            new CommonData { Value = AccountingConstants.PAYMENT_TYPE_CODE_PAY_OTHER, DisplayName = AccountingConstants.PAYMENT_TYPE_NAME_PAY_OTHER },
            new CommonData { Value = AccountingConstants.PAYMENT_TYPE_CODE_COLLECT_OBH, DisplayName = AccountingConstants.PAYMENT_TYPE_NAME_COLLECT_OBH },
            new CommonData { Value = AccountingConstants.PAYMENT_TYPE_CODE_COLLECT_OTHER, DisplayName = AccountingConstants.PAYMENT_TYPE_NAME_COLLECT_OTHER },
            new CommonData { Value = AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE, DisplayName = AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE }
        };
    }
}
