using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatHub
    {
        public Guid HubId { get; set; }
        public int IncreasingId { get; set; }
        public short Buid { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public short? CountryId { get; set; }
        public string PostalCode { get; set; }
        public Guid? ProvinceId { get; set; }

        public CatCountry Country { get; set; }
    }
}
