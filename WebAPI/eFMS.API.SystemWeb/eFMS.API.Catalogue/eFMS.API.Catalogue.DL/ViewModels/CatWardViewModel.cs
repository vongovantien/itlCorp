using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatWardViewModel
    {
        public short Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CodeDistrict { get; set; }
        public string DistrictName { get; set; }
        public string CodeCity { get; set; }
        public string CityName { get; set; }
        public string CodeCountry { get; set; }
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
