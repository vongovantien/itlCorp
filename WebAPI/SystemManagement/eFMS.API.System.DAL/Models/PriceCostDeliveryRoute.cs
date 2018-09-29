using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceCostDeliveryRoute
    {
        public Guid Id { get; set; }
        public Guid CostId { get; set; }
        public Guid DeliveryPlaceId { get; set; }
        public short DeliveryZoneId { get; set; }
        public short? VehicleTypeId { get; set; }
        public int? WeightRangeId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public int? Length { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadAmount { get; set; }
        public int? ShipmentsPerDay { get; set; }
        public decimal? WeightPerShipment { get; set; }
        public decimal? TotalChargedWeight { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalUnitCost { get; set; }
        public decimal? Mnramount { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string Notes { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
