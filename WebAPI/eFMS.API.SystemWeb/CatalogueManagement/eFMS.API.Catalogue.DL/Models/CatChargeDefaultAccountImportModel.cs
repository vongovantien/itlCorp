using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeDefaultAccountImportModel:CatChargeDefaultAccountModel
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeCodeError { get; set; }
        public string DebitAccountNoError { get; set; }
        public string CreditAccountNoError { get; set; }
        public string DebitVatError { get; set; }
        public string CreditVatError { get; set; }
        public string TypeError { get; set; }
    }
}
