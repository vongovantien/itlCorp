using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleFclquotationShortTrip
    {
        public Guid Id { get; set; }
        public Guid QuotationRouteId { get; set; }
        public Guid RouteId { get; set; }
        public int LenthKm { get; set; }
        public short? VehicleTypeId { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string SupplierId { get; set; }
        public Guid? BuyingPriceId { get; set; }
        public Guid? TripBuyingRouteId { get; set; }
        public decimal? BuyingPrice { get; set; }
        public Guid? CostingPriceId { get; set; }
        public short NumberOfDays { get; set; }
        public short NumberOfTrips { get; set; }
        public decimal? Kratio { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadCost { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? ChargeCost { get; set; }
        public decimal? Mnrcost { get; set; }
        public decimal? MinCost { get; set; }
        public string CurrencyId { get; set; }
        public bool? BelongToCustomer { get; set; }
        public int? Repetition { get; set; }
        public int? SequentialNumber { get; set; }
        public string JourneyNote { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public decimal? FuelAllowance { get; set; }
        public short? FuelAllowanceUnit { get; set; }
        public string FuelAllowanceNote { get; set; }
    }
}
