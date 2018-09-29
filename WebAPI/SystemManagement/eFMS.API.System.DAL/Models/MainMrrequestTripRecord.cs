using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainMrrequestTripRecord
    {
        public Guid Id { get; set; }
        public Guid MrrequestId { get; set; }
        public int Trip { get; set; }
        public string Place { get; set; }
        public int? ContermetNumberDriver { get; set; }
        public int? ContermetNumber { get; set; }
        public int? DriverLength { get; set; }
        public int? Length { get; set; }
        public decimal? FuelAmount { get; set; }
        public decimal? TotalFuelPrice { get; set; }
        public string Notes { get; set; }
        public DateTime? DatetimeTripUpdate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public decimal? FuelPrice { get; set; }
    }
}
