using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehicleGroup
    {
        public CatVehicleGroup()
        {
            CatVehiclePart = new HashSet<CatVehiclePart>();
            CatVehicleType = new HashSet<CatVehicleType>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string GroupNameVn { get; set; }
        public string GroupNameEn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactivenOn { get; set; }

        public ICollection<CatVehiclePart> CatVehiclePart { get; set; }
        public ICollection<CatVehicleType> CatVehicleType { get; set; }
    }
}
