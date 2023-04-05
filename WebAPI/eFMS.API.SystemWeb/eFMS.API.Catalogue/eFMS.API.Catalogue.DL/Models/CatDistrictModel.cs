using eFMS.API.Catalogue.Service.Models;


namespace eFMS.API.Catalogue.DL.Models
{
    public class CatDistrictModel : CatDistrict
    {
        public string ProvinceNameEN { get; set; }
        public string ProvinceNameVN { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
