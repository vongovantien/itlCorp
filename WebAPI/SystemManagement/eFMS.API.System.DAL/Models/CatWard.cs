using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatWard
    {
        public Guid WardId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid? BelongToBranch { get; set; }
        public short PickupZoneCode { get; set; }
        public string PostalCode { get; set; }
    }
}
