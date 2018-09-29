using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysGpsprovider
    {
        public SysGpsprovider()
        {
            CatVehicle = new HashSet<CatVehicle>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Wsurl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CatVehicle> CatVehicle { get; set; }
    }
}
