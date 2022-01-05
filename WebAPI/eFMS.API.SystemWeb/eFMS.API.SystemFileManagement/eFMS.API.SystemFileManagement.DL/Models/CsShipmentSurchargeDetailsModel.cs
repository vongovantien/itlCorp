using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class CsShipmentSurchargeDetailsModel: CsShipmentSurchargeModel
    {
        public string NameEn { get; set; }
        public string Currency { get; set; }
        public string ChargeCode { get; set; }
        public string Type { get; set; }
        public string BillingType { get; set; }
    }
}
