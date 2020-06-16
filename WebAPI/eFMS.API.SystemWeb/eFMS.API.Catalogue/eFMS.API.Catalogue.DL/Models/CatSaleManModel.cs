using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatSaleManModel : CatSaleman
    {
        public string Username { get; set; }
        public string OfficeNameEn { get; set; }
        public string OfficeNameVn { get; set; }
        public string OfficeNameAbbr{ get; set; }
        public string CompanyNameEn { get; set; }
        public string CompanyNameVn { get; set; }
        public string CompanyNameAbbr{ get; set; }
    }
}
