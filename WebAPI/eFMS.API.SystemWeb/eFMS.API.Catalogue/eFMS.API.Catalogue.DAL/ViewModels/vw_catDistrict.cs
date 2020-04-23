using System;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_catDistrict
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name_VN { get; set; }
        public string Name_EN { get; set; }
        public Nullable<Guid> ProvinceID { get; set; }
        public string ProvinceNameEN { get; set; }
        public string ProvinceNameVN { get; set; }
        public Nullable<short> CountryID { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<DateTime> InActiveOn { get; set; }
    }
}
