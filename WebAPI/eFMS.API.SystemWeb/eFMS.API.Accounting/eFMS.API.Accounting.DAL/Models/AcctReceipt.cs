using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AcctReceipt
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string PaymentRefNo { get; set; }
        public string Status { get; set; }
        public Guid? AgreementId { get; set; }
        public decimal? PaidAmount { get; set; }
        public string Type { get; set; }
        public decimal? CusAdvanceAmount { get; set; }
        public decimal? FinalPaidAmount { get; set; }
        public string Balance { get; set; }
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
    }
}
