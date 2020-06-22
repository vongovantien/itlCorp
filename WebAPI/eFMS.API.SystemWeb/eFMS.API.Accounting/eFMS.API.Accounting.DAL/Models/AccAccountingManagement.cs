using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AccAccountingManagement
    {
        public Guid Id { get; set; }
        public string PartnerId { get; set; }
        public string PersonalName { get; set; }
        public string PartnerAddress { get; set; }
        public string Description { get; set; }
        public string VoucherId { get; set; }
        public DateTime? Date { get; set; }
        public string InvoiceNoTempt { get; set; }
        public string InvoiceNoReal { get; set; }
        public string Serie { get; set; }
        public string PaymentMethod { get; set; }
        public string VoucherType { get; set; }
        public string AccountNo { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string AttachDocInfo { get; set; }
        public string Type { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string PaymentStatus { get; set; }
        public int? PaymentExtendDays { get; set; }
        public string PaymentNote { get; set; }
        public DateTime? PaymentDatetimeUpdated { get; set; }
    }
}
