using eFMS.API.Catalogue.Service.Models;
using System;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatStandardChargeModel : CatStandardCharge
    {
        public string Code { get; set; }
        public string UnitCode { get; set; }
        public Guid? ChargeGroup { get; set; }
    }
}
