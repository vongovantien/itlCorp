using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceRouteCost1
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid RouteId { get; set; }
        public short VehicleTypeId { get; set; }
        public int WeightRangeId { get; set; }
        public int? Length { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadAmount { get; set; }
        public decimal? Mnramount { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? GuaranteedDistancePerMonth { get; set; }
        public decimal? TripAllowance { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public int? ShipmentsPerDay { get; set; }
        public decimal? WeightPerShipment { get; set; }
        public decimal? ChargedWeight { get; set; }
        public short? UnitId { get; set; }
        public decimal? UnitCost { get; set; }
        public short UnitForCalculation { get; set; }
        public string Note { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ApprovedUer { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public short? Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public CatBranch Branch { get; set; }
    }
}
