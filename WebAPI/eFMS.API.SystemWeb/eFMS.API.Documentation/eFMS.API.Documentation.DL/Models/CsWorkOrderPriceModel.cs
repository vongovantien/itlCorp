using eFMS.API.Documentation.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsWorkOrderPriceModel: CsWorkOrderPrice
    {
        public List<CsWorkOrderSurchargeModel> Surcharges { get; set; }
        public string ChargeCodeBuying { get; set; }
        public string ChargeCodeSelling { get; set; }
        public string UnitCode { get; set; }
        public string PartnerName { get; set; }
        public string TransactionType { get; set; }
    }
}
