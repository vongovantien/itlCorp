using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class CatPlaceCriteria
    {
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
        public short? CountryId { get; set; }
        public string AreaId { get; set; }
        public string LocalAreaId { get; set; }
        public string ModeOfTransport { get; set; }
        public string GeoCode { get; set; }
        public string PlaceTypeId { get; set; }
        public string Note { get; set; }
    }
}
