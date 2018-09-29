using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceFclcost
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid RouteId { get; set; }
        public string ContainerTypeId { get; set; }
        public int WeightRangeId { get; set; }
        public short? VehicleTypeId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public int? Length { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadCost { get; set; }
        public decimal? Mnrcost { get; set; }
        public decimal? TotalSurcharge { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string Notes { get; set; }
        public DateTime? EffectedDateFrom { get; set; }
        public DateTime? EffectedDateTo { get; set; }
        public string UserActive { get; set; }
        public DateTime? ActiveOn { get; set; }
        public short? Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
