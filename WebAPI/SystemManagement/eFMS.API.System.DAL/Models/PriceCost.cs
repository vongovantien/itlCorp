using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceCost
    {
        public Guid Id { get; set; }
        public Guid OriginBranchId { get; set; }
        public Guid DestinationBranchId { get; set; }
        public int ServiceTypeMappingId { get; set; }
        public bool? ConsolidatedAtHub { get; set; }
        public decimal? PickupCost { get; set; }
        public decimal? TransitCost { get; set; }
        public decimal? Days { get; set; }
        public decimal? Kratio { get; set; }
        public short DefaultWeightUnit { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public CatBranch DestinationBranch { get; set; }
        public CatBranch OriginBranch { get; set; }
    }
}
