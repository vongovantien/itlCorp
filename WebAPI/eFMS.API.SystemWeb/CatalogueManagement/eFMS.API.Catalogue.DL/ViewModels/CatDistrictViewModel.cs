using System;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatDistrictViewModel
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Nullable<Guid> ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public Nullable<short> CountryID { get; set; }
        public string CountryName { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<DateTime> InActiveOn { get; set; }
    }
}
