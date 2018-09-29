using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainMrrequest
    {
        public Guid Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string Code { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public int? RemoocId { get; set; }
        public string RequestedVehicleType { get; set; }
        public int? MaintenancePlaceId { get; set; }
        public int? ContermetNumber { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string ApprovedManager { get; set; }
        public int? ApprovedStatus { get; set; }
        public string ApprovedNotes { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public short? Status { get; set; }
        public bool? SentSms { get; set; }
        public DateTime? FinishedDate { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public string Reason { get; set; }
        public string Remark { get; set; }
        public string PoStart { get; set; }
        public string PoEnd { get; set; }
        public decimal? TotalLengthGps { get; set; }
        public decimal? FuelConsumption { get; set; }
        public decimal? TotalFuelPrice { get; set; }
        public decimal? TotalFuelLiter { get; set; }
        public bool? CheckedFuel { get; set; }
        public string CheckedFuelUser { get; set; }
        public DateTime? CheckedFuelDate { get; set; }
        public string CheckedFuelNote { get; set; }
        public bool? LockFuel { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
