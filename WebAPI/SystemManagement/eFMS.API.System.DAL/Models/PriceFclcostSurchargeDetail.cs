using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceFclcostSurchargeDetail
    {
        public Guid CostId { get; set; }
        public string ChargeId { get; set; }
        public decimal Price { get; set; }
        public string CurrencyId { get; set; }
        public bool? IncludedVat { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
