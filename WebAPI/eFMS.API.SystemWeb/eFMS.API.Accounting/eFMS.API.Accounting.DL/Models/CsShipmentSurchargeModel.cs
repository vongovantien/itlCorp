using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class CsShipmentSurchargeModel:CsShipmentSurcharge
    {
        public string Pic { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeNameEn { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
