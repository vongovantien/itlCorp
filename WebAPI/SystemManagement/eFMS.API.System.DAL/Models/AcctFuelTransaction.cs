using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AcctFuelTransaction
    {
        public Guid Id { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Guid? TransportId { get; set; }
        public Guid? RouteId { get; set; }
        public decimal? FuelAmount { get; set; }
        public decimal? ActualFuelAmount { get; set; }
        public decimal? FuelPrice { get; set; }
        public decimal? TransactionAmount { get; set; }
        public string RefNo { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public string PaymentMethod { get; set; }
        public string PetrolStationId { get; set; }
        public string Notes { get; set; }
        public decimal? LiterConsumption { get; set; }
        public DateTime? PaidOn { get; set; }
        public string PaidBy { get; set; }
        public decimal? TotalShipmentWeight { get; set; }
        public decimal? TotalWeightFuelCs { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public short? Length { get; set; }
        public bool? Printed { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string AccountNote { get; set; }
    }
}
