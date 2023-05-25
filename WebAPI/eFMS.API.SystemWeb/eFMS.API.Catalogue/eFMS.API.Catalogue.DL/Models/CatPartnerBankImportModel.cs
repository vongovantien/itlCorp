namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerBankImportModel : CatPartnerBankModel
    {
        public string CustomerCode { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

    }
}
