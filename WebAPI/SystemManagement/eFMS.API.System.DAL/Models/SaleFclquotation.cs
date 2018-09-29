using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleFclquotation
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Code { get; set; }
        public string CustomerId { get; set; }
        public bool? IsPublicQuotation { get; set; }
        public string CustomerContact { get; set; }
        public string Tel { get; set; }
        public string Note { get; set; }
        public bool? IsTrial { get; set; }
        public DateTime? FromTrialDate { get; set; }
        public DateTime? ToTrialDate { get; set; }
        public string QuotationType { get; set; }
        public bool? CustomerApproved { get; set; }
        public string RejectionReason { get; set; }
        public string ToUser { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public int? ContractId { get; set; }
        public string ReferCustomerId { get; set; }
        public decimal? ReferCustomerPercent { get; set; }
    }
}
