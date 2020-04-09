using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeImportModel:CatChargeModel
    {
        public string UnitCode { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string ServiceName { get; set; }
        public string ServiceNameError { get; set; }
        public string CodeError { get; set; }
        public string ChargeNameVnError { get; set; }
        public string ChargeNameEnError { get; set; }
        public string TypeError { get; set; }
        public string CurrencyError { get; set; }
        public string UnitPriceError { get; set; }
        public string UnitError { get; set; }
        public string VatrateError { get; set; }
    }
}
