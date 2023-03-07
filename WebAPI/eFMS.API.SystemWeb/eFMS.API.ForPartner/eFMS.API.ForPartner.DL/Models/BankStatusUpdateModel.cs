using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Models
{
    public class BankStatusUpdateModel
    {
        public string PartnerCode { get; set; }
        public List<CatBankStatusUpdateModel> BankInfo { get; set; }
    }
}
