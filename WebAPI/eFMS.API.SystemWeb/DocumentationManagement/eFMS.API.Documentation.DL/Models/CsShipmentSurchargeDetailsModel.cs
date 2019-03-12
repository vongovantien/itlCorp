using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShipmentSurchargeDetailsModel : CsShipmentSurchargeModel
    {
        public string PartnerName { get; set; }
        public string NameEn { get; set; }
        public string ReceiverName { get; set; }
        public string PayerName { get; set; }
        public string Unit { get; set; }
        public string Currency { get; set; }
        public string ChargeCode { get; set; }
    }
}
