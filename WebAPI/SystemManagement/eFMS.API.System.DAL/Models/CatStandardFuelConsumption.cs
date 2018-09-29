using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatStandardFuelConsumption
    {
        public int Id { get; set; }
        public Guid BranchId { get; set; }
        public short? VehicleTypeId { get; set; }
        public int? WeightRangeId { get; set; }
        public string RouteType { get; set; }
        public string HaulType { get; set; }
        public decimal? Consumption { get; set; }
        public decimal? AdditionalWeight { get; set; }
        public bool? IsDefault { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
