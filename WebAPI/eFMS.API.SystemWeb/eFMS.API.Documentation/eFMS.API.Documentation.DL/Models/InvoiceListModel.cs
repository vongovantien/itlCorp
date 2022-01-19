using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class InvoiceListModel
    {
        public Guid Id { get; set; }
        public Guid? JobId { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string ReferenceNo { get; set; }
        public Guid? HBLId { get; set; }
        public string JobNo { get; set; }
        public string HBLNo { get; set; }
        public decimal? Total { get; set; }
        public string Currency { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string Creator { get; set; }
        public string Status { get; set; }
        public string InvoiceNo { get; set; }
        public string VoucherId { get; set; }
        public string IssuedStatus { get; set; }
        public string SyncStatus { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string MBLNo { get; set; }
        public string CodeNo { get; set; }
        public string CodeType { get; set; }
        public string ChargeType { get; set; }
        public string PayerId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? VoucherIddate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string AccountNo { get; set; }
        public string InvType { get; set; }
        public string PaymentStatus { get; set; }

    }
}
