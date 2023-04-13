﻿using eFMS.API.Catalogue.Service.Models;


namespace eFMS.API.Catalogue.DL.Models
{
    public class CatDistrictModel : CatDistrict
    {
        public string CodeCountry { get; set; }
        public string CodeCity { get; set; }
        public string CountryName { get; set; }
        public string ProvinceName { get; set; }
        public string CodeError { get; set; } 
        public string NameVnError { get; set; }
        public string NameEnError { get; set; }
        public string CodeCountryError { get; set; }
        public string ProvinceNameError { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
