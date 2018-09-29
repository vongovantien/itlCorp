using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsShipmentChecking
    {
        public int Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public Guid CheckedLocation { get; set; }
        public string CheckedUser { get; set; }
        public DateTime? CheckedOn { get; set; }
        public string CheckingType { get; set; }
        public bool? LoadedToVehicle { get; set; }
    }
}
