using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehicleType
    {
        public CatVehicleType()
        {
            CatVehicle = new HashSet<CatVehicle>();
            SysParameterVehicleType = new HashSet<SysParameterVehicleType>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string TypeNameVn { get; set; }
        public string TypeNameEn { get; set; }
        public short VehicleGroupId { get; set; }
        public decimal? MaxWeight { get; set; }
        public short? UnitId { get; set; }
        public string ShipmentTypeId { get; set; }
        public string HaulType { get; set; }
        public bool? IsDefault { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactivenOn { get; set; }
        public string ShortName { get; set; }

        public CatVehicleGroup VehicleGroup { get; set; }
        public ICollection<CatVehicle> CatVehicle { get; set; }
        public ICollection<SysParameterVehicleType> SysParameterVehicleType { get; set; }
    }
}
