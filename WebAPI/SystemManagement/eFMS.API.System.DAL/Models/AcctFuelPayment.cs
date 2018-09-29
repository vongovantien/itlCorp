using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AcctFuelPayment
    {
        public Guid Id { get; set; }
        public string PaymentRefNo { get; set; }
        public Guid? TransactionId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? DriverId { get; set; }
        public decimal? FuelCostConsumption { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? FuelAmount { get; set; }
        public decimal? TransactionAmount { get; set; }
        public decimal? DifferentLiter { get; set; }
        public decimal? DifferentFuelCost { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? PriceUnit { get; set; }
        public decimal? DiscountFuel { get; set; }
        public string Status { get; set; }
        public string OpsmanId { get; set; }
        public DateTime? OpsmanDate { get; set; }
        public string OpsmanStatus { get; set; }
        public string OpsmanNote { get; set; }
        public string ChiefId { get; set; }
        public DateTime? ChiefDate { get; set; }
        public string ChiefStatus { get; set; }
        public string ChiefNote { get; set; }
        public string AccountantId { get; set; }
        public DateTime? AccountantDate { get; set; }
        public string AccountantStatus { get; set; }
        public string AccountantNote { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid? WorkPlaceId { get; set; }
    }
}
