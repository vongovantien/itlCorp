using System;
using SystemManagementAPI.Service.Contexts;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Models.Views
{
    public class vw_catPlace
    {
        public Guid ID { get; set; }

        public Guid? BranchID { get; set; }

        public string Code { get; set; }

        public string Name_VN { get; set; }

        public string Name_EN { get; set; }

        public string PlaceTypeID { get; set; }

        public string DisplayName { get; set; }

        public string PlaceTypeName { get; set; }

        public string Note { get; set; }

        public string Address { get; set; }

        public Guid? DistrictID { get; set; }

        public string DistrictName_VN { get; set; }

        public Guid? ProvinceID { get; set; }

        public string ProvinceName_VN { get; set; }

        public string AreaID { get; set; }

        public string AreaName_VN { get; set; }

        public string AreaName_EN { get; set; }

        public short? CountryID { get; set; }

        public string GeoCode { get; set; }

        public string UserCreated { get; set; }

        public DateTime? DatetimeCreated { get; set; }

        public string UserModified { get; set; }

        public DateTime? DatetimeModified { get; set; }

        public bool? Inactive { get; set; }

        public DateTime? InactiveOn { get; set; }

        public string PlaceTypeName_EN { get; set; }

    }

}
