using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatDistrict
    {
        public Guid DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
    }
}
