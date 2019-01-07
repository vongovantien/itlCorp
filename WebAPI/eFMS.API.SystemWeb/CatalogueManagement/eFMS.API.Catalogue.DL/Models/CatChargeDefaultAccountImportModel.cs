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
    }
}
