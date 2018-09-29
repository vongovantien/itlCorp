using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Models.Views
{
    public partial class vw_WorkPlace
    {
        public System.Guid ID { get; set; }
        public string Code { get; set; }
        public string Name_VN { get; set; }
        public string Name_EN { get; set; }
        public string PlaceTypeID { get; set; }
        public string Note { get; set; }
        public string PlaceTypeName_VN { get; set; }
        public string PlaceTypeName_EN { get; set; }
        public string PublicName_VN { get; set; }
        public string PublicName_EN { get; set; }
        public string Address { get; set; }
        public Nullable<System.Guid> DistrictID { get; set; }
        public string GeoCode { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string UserCreated { get; set; }
        public Nullable<System.DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<System.DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<System.DateTime> InactiveOn { get; set; }
    }
}
