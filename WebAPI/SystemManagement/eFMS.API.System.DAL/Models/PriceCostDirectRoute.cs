using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceCostDirectRoute
    {
        public Guid Id { get; set; }
        public Guid CostZoneMappingId { get; set; }
        public Guid RouteId { get; set; }
        public Guid FromPlaceId { get; set; }
        public Guid ToPlaceId { get; set; }
        public string RoadId { get; set; }
        public short? VehicleTypeId { get; set; }
        public int? WeightRangeId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public int? Length { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadAmount { get; set; }
        public decimal? TotalChargedWeight { get; set; }
        public short? WeightUnitId { get; set; }
        public decimal? Mnramount { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? UnitCost { get; set; }
        public string Notes { get; set; }
        public bool? IsOutSource { get; set; }
        public Guid? BuyingRouteId { get; set; }
        public Guid? FclbuyingId { get; set; }
        public Guid? TripBuyingRouteId { get; set; }
        public string SupplierId { get; set; }
        public string Contract { get; set; }
        public int? SequentialNumber { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
