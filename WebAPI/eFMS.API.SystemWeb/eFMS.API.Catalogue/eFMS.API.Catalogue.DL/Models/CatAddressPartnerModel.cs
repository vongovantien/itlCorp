using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatAddressPartnerModel : CatAddressPartner
    {
        public string AccountNo { get; set; }
        public string ShortName { get; set; }
        public string TaxCode { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string DistrictName { get; set; }
        public string WardName { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
    }
}
