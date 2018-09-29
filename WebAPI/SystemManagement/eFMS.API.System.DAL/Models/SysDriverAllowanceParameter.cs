using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysDriverAllowanceParameter
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public short? VehicleTypeId { get; set; }
        public decimal? FromKm { get; set; }
        public decimal? ToKm { get; set; }
        public decimal Value { get; set; }
        public string CurrencyId { get; set; }
        public bool? IsFullTrip { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
