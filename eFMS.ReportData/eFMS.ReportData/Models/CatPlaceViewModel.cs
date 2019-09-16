using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.ReportData.Models
{
    public class CatPlaceViewModel
    {
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Address { get; set; }
        public Nullable<Guid> DistrictID { get; set; }
        public string DistrictName { get; set; }
        public Nullable<Guid> ProvinceID { get; set; }
        public string ProvinceName { get; set; }
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
        public string CountryName { get; set; }
        public string AreaName { get; set; }
        public string LocalAreaName { get; set; }
    }
}
