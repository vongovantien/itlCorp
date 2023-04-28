using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatAddressPartnerCriteria
    {
        public string All { get; set; }
        public string Id { get; set; }
        public string ShortNameAddress { get; set; }
        public short CountryID { get; set; }
        public Guid CityID { get; set; }
        public Guid DistrictID { get; set; }
        public Guid WardID { get; set; }
        public string StreetAddress { get; set; }
        public string AddressType { get; set; }
        public string Location { get; set; }
        public Guid PartnerId { get; set; } 
        public bool? Active { get; set; }
        public DateTime? InActiveOn { get; set; }
    }
}
