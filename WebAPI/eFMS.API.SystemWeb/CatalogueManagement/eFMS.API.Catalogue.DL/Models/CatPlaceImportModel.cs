using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPlaceImportModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public string DistrictName { get; set; }
        public Guid? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public short? CountryId { get; set; }
        public string CountryName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string LocalAreaId { get; set; }
        public string ModeOfTransport { get; set; }
        public string PlaceTypeId { get; set; }
        public string Note { get; set; }
        public bool? Active { get; set; }
        public DateTime? InActiveOn { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string CountryNameError { get; set; }
        public string CodeError { get; set; }
        public string NameEnError { get; set; }
        public string NameVnError { get; set; }
        public string ProvinceNameError { get; set; }
        public string DistrictNameError { get; set; }
        public string DisplayNameError { get; set; }
        public string AddressError { get; set; }
        public string AreaNameError { get; set; }
        public string ModeOfTransportError { get; set; }
    }
}
