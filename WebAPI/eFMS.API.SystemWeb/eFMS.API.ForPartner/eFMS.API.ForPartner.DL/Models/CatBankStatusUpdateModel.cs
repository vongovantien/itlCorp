using System.ComponentModel.DataAnnotations;

namespace eFMS.API.ForPartner.DL.Models
{
    public class CatBankStatusUpdateModel
    {
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string BankAccountno { get; set; }
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [RegularExpression("Approved|Rejected", ErrorMessage = "EF_ANNOTATIONS_NOT_VALID")]
        public string ApproveStatus { get; set; }
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string Description { get; set; }
    }
}
