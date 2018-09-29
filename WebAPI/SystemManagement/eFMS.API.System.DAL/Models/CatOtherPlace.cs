using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatOtherPlace
    {
        public Guid PlaceId { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public short? CountryId { get; set; }
        public Guid BranchId { get; set; }
        public short PickupZoneCode { get; set; }
        public decimal? FuelAllowance { get; set; }
        public short? FuelAllowanceUnit { get; set; }
        public Guid? ProvinceId { get; set; }
        public string SupplierId { get; set; }
    }
}
