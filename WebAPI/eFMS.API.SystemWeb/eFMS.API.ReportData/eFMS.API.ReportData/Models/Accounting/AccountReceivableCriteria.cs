﻿using eFMS.API.ReportData.Models.Common.Enums;
using System;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class AccountReceivableCriteria
    {
        public ARTypeEnum ArType { get; set; }
        public string AcRefId { get; set; }
        public OverDueDayEnum OverDueDay { get; set; }
        public decimal? DebitRateFrom { get; set; }
        public decimal? DebitRateTo { get; set; }
        public string AgreementStatus { get; set; } //All, Active, Inactive
        public string AgreementExpiredDay { get; set; } //All, Normal, 30Day, 15Day, Expried
        public string SalesmanId { get; set; }
        public Guid? OfficeId { get; set; }
    }
}