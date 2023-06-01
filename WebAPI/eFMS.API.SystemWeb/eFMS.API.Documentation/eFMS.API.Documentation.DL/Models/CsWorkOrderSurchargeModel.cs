using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsWorkOrderSurchargeModel: CsWorkOrderSurcharge
    {
        public string PartnerName { get; set; }
        public string ChargeName { get; set; }
        public string UnitCode { get; set; }
    }
}
