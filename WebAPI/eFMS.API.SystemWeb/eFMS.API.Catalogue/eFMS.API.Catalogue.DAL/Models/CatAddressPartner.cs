using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatAddressPartner
    {
        public Guid Id { get; set; }
        public string ShortNameAddress { get; set; }
        public string StreetAddress { get; set; }
        public string AddressType { get; set; }
        public string Location { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public Guid? PartnerId { get; set; }
        public string ContactPerson { get; set; }
        public string Tel { get; set; }
        public short? CountryId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? WardId { get; set; }
    }
}
