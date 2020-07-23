using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeIncoterm
    {
        public Guid ChargeId { get; set; }
        public int UnitId { get; set; }
        public string ChargeTo { get; set; }
        public string CurrencyId { get; set; }
        public string FeeType { get; set; }
    }
}
