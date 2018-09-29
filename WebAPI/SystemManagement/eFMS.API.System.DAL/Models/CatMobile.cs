using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatMobile
    {
        public string PhoneNumber { get; set; }
        public string Imei1 { get; set; }
        public string Imei2 { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public string AppVersion { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
