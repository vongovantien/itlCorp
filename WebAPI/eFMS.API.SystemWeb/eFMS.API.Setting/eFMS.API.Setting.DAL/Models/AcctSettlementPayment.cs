using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class AcctSettlementPayment
    {
        public Guid Id { get; set; }
        public string SettlementNo { get; set; }
        public string Requester { get; set; }
        public DateTime? RequestDate { get; set; }
        public string PaymentMethod { get; set; }
        public string SettlementCurrency { get; set; }
        public string StatusApproval { get; set; }
        public string LockedLog { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public decimal? Amount { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string VoucherNo { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string SyncStatus { get; set; }
        public string ReasonReject { get; set; }
        public string Payee { get; set; }
    }
}
