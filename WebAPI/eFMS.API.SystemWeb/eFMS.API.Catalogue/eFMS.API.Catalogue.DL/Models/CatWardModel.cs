using eFMS.API.Catalogue.Service.Models;
namespace eFMS.API.Catalogue.DL.Models
{
    public class CatWardModel : CatWard
    {
        public string CodeVallid { get; set; }
        public string NameVnValid { get; set; }
        public string NameEnValid { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
