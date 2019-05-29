using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_catPlace
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public Nullable<Guid> DistrictID { get; set; }
        public string DistrictNameEN { get; set; }
        public string DistrictNameVN { get; set; }
        public Nullable<Guid> ProvinceID { get; set; }
        public string ProvinceNameEN { get; set; }
        public string ProvinceNameVN { get; set; }
        public Nullable<short> CountryID { get; set; }
        public string AreaID { get; set; }
        public string LocalAreaID { get; set; }
        public string ModeOfTransport { get; set; }
        public string GeoCode { get; set; }
        public string PlaceTypeID { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<DateTime> InactiveOn { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public string AreaNameVN { get; set; }
        public string AreaNameEN { get; set; }
        public string LocalAreaNameEN { get; set; }
        public string LocalAreaNameVN { get; set; }
    }
}
