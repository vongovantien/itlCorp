using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatUnit
    {
        public CatUnit()
        {
            CatUnitExchangeUnitFromNavigation = new HashSet<CatUnitExchange>();
            CatUnitExchangeUnitToNavigation = new HashSet<CatUnitExchange>();
            CatVehiclePart = new HashSet<CatVehiclePart>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string UnitNameVn { get; set; }
        public string UnitNameEn { get; set; }
        public string UnitType { get; set; }
        public string ShipmentTypeId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public CatShipmentType ShipmentType { get; set; }
        public ICollection<CatUnitExchange> CatUnitExchangeUnitFromNavigation { get; set; }
        public ICollection<CatUnitExchange> CatUnitExchangeUnitToNavigation { get; set; }
        public ICollection<CatVehiclePart> CatVehiclePart { get; set; }
    }
}
