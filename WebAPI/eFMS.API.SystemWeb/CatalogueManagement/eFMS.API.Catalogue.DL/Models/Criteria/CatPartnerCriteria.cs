using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatPartnerCriteria
    {
        public string All { get; set; }
        public CatPartnerGroupEnum PartnerGroup { get; set; }
        public string Id { get; set; }
        public string ShortName { get; set; }
        public string AddressVn { get; set; }
        public string AddressEn { get; set; }
        public string TaxCode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
    }
}
