﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.Service.Models
{
    public partial class AcctAdvancePayment
    {
        public Guid Id { get; set; }
        public string AdvanceNo { get; set; }
        public string Requester { get; set; }
        public string Department { get; set; }
        public string PaymentMethod { get; set; }
        public string AdvanceCurrency { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DeadlinePayment { get; set; }
        public string StatusApproval { get; set; }
        public string LockedLog { get; set; }
        public string AdvanceNote { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string SyncStatus { get; set; }
        public decimal? PaymentTerm { get; set; }
        public string ReasonReject { get; set; }
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
        public string Payee { get; set; }
        public decimal? ExcRateUsdToLocal { get; set; }
        public string BankCode { get; set; }
        public string AdvanceFor { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
