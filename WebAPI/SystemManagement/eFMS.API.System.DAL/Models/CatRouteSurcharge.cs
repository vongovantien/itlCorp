using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatRouteSurcharge
    {
        public Guid Id { get; set; }
        public Guid RouteId { get; set; }
        public string ChargeId { get; set; }
        public short? VehicleTypeId { get; set; }
        public int WeightRangeId { get; set; }
        public string ContainerTypeId { get; set; }
        public decimal Price { get; set; }
        public string CurrencyId { get; set; }
        public bool? IncludedVat { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
