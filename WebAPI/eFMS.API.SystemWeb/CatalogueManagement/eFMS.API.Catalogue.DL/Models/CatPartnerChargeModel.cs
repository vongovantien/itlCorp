using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerChargeModel: CatPartnerCharge
    {
        public string partnerName { get; set; }
        public string chargeNameEn { get; set; }
        public string partnerShortName { get; set; }
    }
}
