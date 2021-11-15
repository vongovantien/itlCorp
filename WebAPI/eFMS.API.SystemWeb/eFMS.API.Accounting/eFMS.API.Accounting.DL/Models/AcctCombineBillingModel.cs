using eFMS.API.Accounting.DL.Models.CombineBilling;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctCombineBillingModel : AcctCombineBilling
    {
        public List<CombineBillingShipmentModel> Shipments { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
    }
}
