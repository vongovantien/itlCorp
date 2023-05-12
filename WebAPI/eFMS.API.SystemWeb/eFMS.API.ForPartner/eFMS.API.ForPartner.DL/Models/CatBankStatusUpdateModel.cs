using eFMS.API.ForPartner.DL.Anotations;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.ForPartner.DL.Models
{
    public class CatBankStatusUpdateModel
    {
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string BankAccountno { get; set; }
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        [StringContainAttribute(AllowableValues = new string[] {
            "Approved",
            "Rejected",
        })]
        public string ApproveStatus { get; set; }
        [Required(ErrorMessage = "EF_ANNOTATIONS_REQUIRED")]
        public string Description { get; set; }
    }
}
