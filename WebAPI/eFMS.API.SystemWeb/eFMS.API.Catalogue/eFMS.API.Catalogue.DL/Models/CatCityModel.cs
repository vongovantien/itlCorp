using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatCityModel : CatCity
    {
        public string CodeError { get; set; }
        public string CodeCountry { get; set; }
        public string CountryName { get; set; }
        public string NameVnError { get; set; }
        public string NameEnError { get; set; }
        public string CodeCountryError { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
    }
}
