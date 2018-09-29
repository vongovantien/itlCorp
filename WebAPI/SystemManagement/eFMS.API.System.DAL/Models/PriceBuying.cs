using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceBuying
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string SupplierId { get; set; }
        public string CustomerId { get; set; }
        public string Desciption { get; set; }
        public string Contract { get; set; }
        public int? ContractId { get; set; }
        public string CurrencyId { get; set; }
        public string Type { get; set; }
        public string GettingPriceMethod { get; set; }
        public decimal? VolumnWeightRate { get; set; }
        public string AppliedCustomerType { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public DateTime? ExpiryOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string ReferCustomerId { get; set; }
        public decimal? ReferCustomerPercent { get; set; }
    }
}
