using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatShipmentType
    {
        public CatShipmentType()
        {
            CatPlaceDistance = new HashSet<CatPlaceDistance>();
            CatUnit = new HashSet<CatUnit>();
        }

        public string Id { get; set; }
        public string TypeNameVn { get; set; }
        public string TypeNameEn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactivenOn { get; set; }

        public ICollection<CatPlaceDistance> CatPlaceDistance { get; set; }
        public ICollection<CatUnit> CatUnit { get; set; }
    }
}
