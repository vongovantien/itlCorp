using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeImportModel:CatChargeModel
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
