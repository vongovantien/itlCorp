using eFMS.API.Catalogue.Service.Models;
namespace eFMS.API.Catalogue.DL.Models
{
    public class CatWardModel : CatWard
    {
        public string DistrictNameEN { get; set; }
        public string DistrictNameVN { get; set; }
        public string ProvinceNameEN { get; set; }
        public string ProvinceNameVN { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
