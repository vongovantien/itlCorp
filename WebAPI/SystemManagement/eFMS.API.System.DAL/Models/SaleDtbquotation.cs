using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleDtbquotation
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Code { get; set; }
        public string CustomerId { get; set; }
        public int? ContractId { get; set; }
        public string Tel { get; set; }
        public string QuotationScope { get; set; }
        public string SaleMember { get; set; }
        public string SaleResource { get; set; }
        public int? PaymentDeadline { get; set; }
        public string PaymentDeadlineUnit { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
