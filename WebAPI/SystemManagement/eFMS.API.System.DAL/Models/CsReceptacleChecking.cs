using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsReceptacleChecking
    {
        public int Id { get; set; }
        public int ReceptacleMasterId { get; set; }
        public Guid CheckedLocation { get; set; }
        public string CheckedUser { get; set; }
        public DateTime? CheckedOn { get; set; }
        public string CheckingType { get; set; }
        public bool? LoadedToVehicle { get; set; }

        public CatPlace CheckedLocationNavigation { get; set; }
        public CsReceptacleMaster ReceptacleMaster { get; set; }
    }
}
