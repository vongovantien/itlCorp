using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.Models
{
    public class CatPlaceEditModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "EF_ANNOTATIONS_STRING_LENGTH")]
        public string Code { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "EF_ANNOTATIONS_STRING_LENGTH")]
        public string NameVn { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "EF_ANNOTATIONS_STRING_LENGTH")]
        public string NameEn{ get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? ProvinceId { get; set; }
        public short? CountryId { get; set; }
        public string AreaId { get; set; }
        public string LocalAreaId { get; set; }
        public string ModeOfTransport { get; set; }
        public string GeoCode { get; set; }
        public CatPlaceTypeEnum PlaceType { get; set; }
        public string PlaceTypeId { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InInActiveOn { get; set; }
    }
}
