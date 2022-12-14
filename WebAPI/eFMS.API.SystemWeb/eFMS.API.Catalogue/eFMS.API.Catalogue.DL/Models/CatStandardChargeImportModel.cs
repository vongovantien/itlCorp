using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatStandardChargeImportModel : CatStandardChargeModel
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string ServiceError { get; set; }
        public string TypeError { get; set; }
        public string CurrencyError { get; set; }
        public string UnitPriceError { get; set; }
        public string VatrateError { get; set; }

    }
}
