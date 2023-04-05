using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatCityModel : CatCity
    {
        public string CountryNameEN { get; set; }
        public string CountryNameVN { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
