using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleQuotationRouteSurcharge
    {
        public Guid Id { get; set; }
        public Guid QuotationRouteId { get; set; }
        public string ChargeId { get; set; }
        public decimal Price { get; set; }
        public string CurrencyId { get; set; }
        public bool? ChargedToCustomer { get; set; }
        public decimal? OtherRevenuePrice { get; set; }
        public bool? IsRevenue { get; set; }
        public bool? IncludedVat { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
