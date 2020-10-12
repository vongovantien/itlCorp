using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class AcctCdnote
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid JobId { get; set; }
        public Guid BranchId { get; set; }
        public string PartnerId { get; set; }
        public string Type { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public bool? CustomerPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public bool? SentToCustomer { get; set; }
        public string SentByUser { get; set; }
        public DateTime? SentOn { get; set; }
        public decimal? Total { get; set; }
        public DateTime? ExportedDate { get; set; }
        public string UnlockedDirector { get; set; }
        public string UnlockedDirectorStatus { get; set; }
        public DateTime? UnlockedDirectorDate { get; set; }
        public string UnlockedSaleMan { get; set; }
        public string UnlockedSaleManStatus { get; set; }
        public DateTime? UnlockedSaleManDate { get; set; }
        public string InvoiceNo { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public DateTime? StatementDate { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? CustomerConfirmDate { get; set; }
        public short? NumberDayOverDue { get; set; }
        public bool? AlertNumberDayOverDueEmail { get; set; }
        public decimal? PaidPrice { get; set; }
        public decimal? FreightPrice { get; set; }
        public decimal? BehalfPrice { get; set; }
        public decimal? PaidFreightPrice { get; set; }
        public decimal? PaidBehalfPrice { get; set; }
        public string TrackingTransportBill { get; set; }
        public DateTime? TrackingTransportDate { get; set; }
        public string FlexId { get; set; }
        public string Status { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string SyncStatus { get; set; }
    }
}
