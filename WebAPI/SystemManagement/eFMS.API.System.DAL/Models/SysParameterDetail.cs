using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysParameterDetail
    {
        public Guid Id { get; set; }
        public int ParameterId { get; set; }
        public Guid BranchId { get; set; }
        public short? VehicleTypeId { get; set; }
        public string ShipmentTypeId { get; set; }
        public string HaulType { get; set; }
        public decimal Value { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public DateTime? ExpiryOn { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string PartnerId { get; set; }

        public SysParameter Parameter { get; set; }
    }
}
