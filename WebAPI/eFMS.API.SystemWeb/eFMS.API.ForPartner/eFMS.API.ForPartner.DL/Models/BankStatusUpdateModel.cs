using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.ForPartner.DL.Models
{
    public class BankStatusUpdateModel
    {
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string PartnerCode { get; set; }
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public List<CatBankStatusUpdateModel> BankInfo { get; set; }
    }
}
