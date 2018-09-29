using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceFclbuying
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public string RoadId { get; set; }
        public string ContainerTypeId { get; set; }
        public int? WeightRangeId { get; set; }
        public int? ServiceTypeId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public string SupplierId { get; set; }
        public string CustomerId { get; set; }
        public string AppliedCustomerType { get; set; }
        public int? ContractId { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string ReferCustomerId { get; set; }
        public decimal? ReferCustomerPercent { get; set; }
        public decimal? ReferCustomerFee { get; set; }
    }
}
