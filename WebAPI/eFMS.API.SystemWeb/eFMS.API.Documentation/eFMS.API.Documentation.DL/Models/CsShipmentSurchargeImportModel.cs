using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShipmentSurchargeImportModel: CsShipmentSurcharge
    {
        public string HBLNoError { get; set; }
        public string MBLNoError { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerCodeError { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeCodeError { get; set; }
        public string Unit { get; set; }
        public bool IsValid { get; set; }
        public decimal? Qty { get; set; }
        public string QtyError { get; set; }
        public string UnitError { get; set; }
        public string UnitPriceError { get; set; }
        public string CurrencyError { get; set; }
        public string VatError { get; set; }
        public string TotalAmountError { get; set; }
        public string ExchangeDateError{ get; set; }
        public string FinalExchangeDateError { get; set; }
        public string TypeError { get; set; }

        public decimal? TotalAmount { get; set; }
    }
}
