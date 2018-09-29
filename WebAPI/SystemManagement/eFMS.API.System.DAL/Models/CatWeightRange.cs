using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatWeightRange
    {
        public int Id { get; set; }
        public decimal MinWeight { get; set; }
        public decimal MaxWeight { get; set; }
        public short UnitId { get; set; }
        public string ShipmentTypeId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
