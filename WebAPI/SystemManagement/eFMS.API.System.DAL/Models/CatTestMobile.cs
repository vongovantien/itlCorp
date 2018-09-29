using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatTestMobile
    {
        public string PhoneNumber { get; set; }
        public string Imei1 { get; set; }
        public string Imei2 { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public string ConnectionString { get; set; }
    }
}
