using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AcctReceipt
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string PaymentRefNo { get; set; }
        public string Status { get; set; }
        public Guid? AgreementId { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public string Type { get; set; }
        public decimal? FinalPaidAmount { get; set; }
        public decimal? FinalPaidAmountVnd { get; set; }
        public decimal? FinalPaidAmountUsd { get; set; }
        public decimal? Balance { get; set; }
        public string PaymentMethod { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string BankAccountNo { get; set; }
        public string Description { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string ReasonReject { get; set; }
        public string SyncStatus { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string Class { get; set; }
        public Guid? ObhpartnerId { get; set; }
        public string NotifyDepartment { get; set; }
        public decimal? CreditAmountVnd { get; set; }
        public decimal? CreditAmountUsd { get; set; }
        public decimal? CusAdvanceAmountVnd { get; set; }
        public decimal? CusAdvanceAmountUsd { get; set; }
        public decimal? AgreementAdvanceAmountVnd { get; set; }
        public decimal? AgreementAdvanceAmountUsd { get; set; }
        public string ReferenceNo { get; set; }
    }
}
