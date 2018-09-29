using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceDtbstandardCostDetail
    {
        public Guid Id { get; set; }
        public Guid RateCardDetailId { get; set; }
        public Guid? RouteCostId { get; set; }
        public Guid RouteId { get; set; }
        public string RoadId { get; set; }
        public int? WeightRangeId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public int? Length { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadAmount { get; set; }
        public decimal? Mnramount { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? TotalChargedWeight { get; set; }
        public short? WeightUnitId { get; set; }
        public string Notes { get; set; }
        public Guid? BuyingRouteId { get; set; }
        public Guid? FclbuyingId { get; set; }
        public Guid? TripBuyingRouteId { get; set; }
        public string SupplierId { get; set; }
        public string Contract { get; set; }
        public int? SequentialNumber { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
