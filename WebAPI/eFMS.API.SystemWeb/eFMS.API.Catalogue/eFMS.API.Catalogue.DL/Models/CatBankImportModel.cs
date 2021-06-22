using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatBankImportModel:CatBankModel
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string BankName_VN_Error { get; set; }
        public string BankName_EN_Error { get; set; }
        public string CodeError { get;  set; }
    }
}
