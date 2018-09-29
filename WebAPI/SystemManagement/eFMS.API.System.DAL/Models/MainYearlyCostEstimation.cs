using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainYearlyCostEstimation
    {
        public Guid Id { get; set; }
        public Guid MaintenancePlanMasterId { get; set; }
        public int VehiclePartId { get; set; }
        public short VehicleTypeId { get; set; }
        public int Year { get; set; }
        public int? QuantityPerVehicle { get; set; }
        public int? VehicleQuantity { get; set; }
        public int? Frequency { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Amount { get; set; }
        public string CurrencyId { get; set; }
        public string Note { get; set; }
        public string OpsapprovedId { get; set; }
        public string OpsapprovedStatus { get; set; }
        public string OpsapprovedNote { get; set; }
        public DateTime? OpsapprovedDate { get; set; }
        public string ChiefApprovedId { get; set; }
        public string ChiefApprovedStatus { get; set; }
        public string ChiefApprovedNote { get; set; }
        public DateTime? ChiefApprovedDate { get; set; }
        public string HeadApprovedId { get; set; }
        public string HeadApprovedStatus { get; set; }
        public string HeadApprovedNote { get; set; }
        public DateTime? HeadApprovedDate { get; set; }
        public int? Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
