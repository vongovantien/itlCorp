using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatUnitExchange
    {
        public short UnitFrom { get; set; }
        public short UnitTo { get; set; }
        public decimal Rate { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public CatUnit UnitFromNavigation { get; set; }
        public CatUnit UnitToNavigation { get; set; }
    }
}
