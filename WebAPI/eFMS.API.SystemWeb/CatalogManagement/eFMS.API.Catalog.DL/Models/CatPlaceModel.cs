using eFMS.API.Catalog.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalog.DL.Models
{
    public class CatPlaceModel
    {
        public Guid Id { get; set; }
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
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }
        public string CountryName { get; set; }
        public string AreaName { get; set; }
        public string LocalAreaName { get; set; }
        public string PlaceTypeName { get; set; }
    }
}
