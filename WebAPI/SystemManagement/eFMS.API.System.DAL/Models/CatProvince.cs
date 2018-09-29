using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatProvince
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string AreaId { get; set; }
        public short CountryId { get; set; }

        public CatArea Area { get; set; }
        public CatCountry Country { get; set; }
    }
}
