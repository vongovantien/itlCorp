using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatPlaceDistance
    {
        public int Id { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public string ShipmentTypeId { get; set; }
        public short NumberOfDays { get; set; }
        public short NumberOfTrips { get; set; }
        public decimal Kratio { get; set; }
        public int Length { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public Guid? BranchId { get; set; }

        public CatShipmentType ShipmentType { get; set; }
    }
}
