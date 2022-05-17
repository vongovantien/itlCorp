using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsLinkChargeModel : CsLinkCharge
    {
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public string PartnerNameOrg { get; set; }
        public string PartnerNameLink { get; set; }
    }
}
