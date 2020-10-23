using eFMS.API.Documentation.Service.Models;
using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsArrivalFrieghtChargeModel: CsArrivalFrieghtCharge
    {
        public string ChargeName { get; set; }
        public string UnitName { get; set; }
        public string CurrencyName { get; set; }
        public string ChargeCode { get; set; }
        public Guid? ChargeGroup { get; set; }
    }
}
