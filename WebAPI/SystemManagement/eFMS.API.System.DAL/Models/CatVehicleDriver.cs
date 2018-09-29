using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehicleDriver
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public int VehicleId { get; set; }
        public int? RemoocId { get; set; }
        public string Note { get; set; }
        public DateTime ReceiptedDate { get; set; }
        public decimal? ReceivedFuel { get; set; }
        public decimal? ReturnedFuel { get; set; }
        public decimal? DifferenceFuel { get; set; }
        public DateTime? LockedDate { get; set; }
        public decimal? FuelPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid BranchId { get; set; }

        public CatDriver Driver { get; set; }
        public CatVehicle Vehicle { get; set; }
    }
}
