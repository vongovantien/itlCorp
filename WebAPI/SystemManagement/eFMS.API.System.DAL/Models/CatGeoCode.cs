using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatGeoCode
    {
        public int Id { get; set; }
        public Guid? BranchId { get; set; }
        public string GeoCode { get; set; }
        public string Address { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public decimal? FuelAllowance { get; set; }
        public short? FuelAllowanceUnit { get; set; }
        public string FuelAllowanceNote { get; set; }
    }
}
