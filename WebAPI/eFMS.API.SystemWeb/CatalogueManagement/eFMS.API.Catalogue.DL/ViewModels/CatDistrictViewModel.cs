using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatDistrictViewModel
    {
        public System.Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Nullable<System.Guid> ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public Nullable<short> CountryID { get; set; }
        public string CountryName { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public Nullable<System.DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<System.DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<System.DateTime> InactiveOn { get; set; }
    }
}
