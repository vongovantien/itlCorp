using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehiclePart
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string PartNameVn { get; set; }
        public string PartNameEn { get; set; }
        public short? VehicleGroup { get; set; }
        public string UseObject { get; set; }
        public int? VehiclePartTypeId { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public short? UnitId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public CatUnit Unit { get; set; }
        public CatVehicleGroup VehicleGroupNavigation { get; set; }
    }
}
