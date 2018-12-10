using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_WorkPlace
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name_VN { get; set; }
        public string Name_EN { get; set; }
        public string PlaceTypeID { get; set; }
        public string Note { get; set; }
        public string PlaceTypeName_VN { get; set; }
        public string PlaceTypeName_EN { get; set; }
        public string Address_EN { get; set; }
        public string Address_VN { get; set; }
        public Nullable<Guid> DistrictID { get; set; }
        public string GeoCode { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<DateTime> InactiveOn { get; set; }
    }
}
