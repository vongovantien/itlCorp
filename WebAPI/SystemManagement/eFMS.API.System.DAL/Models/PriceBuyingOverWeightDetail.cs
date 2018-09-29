using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceBuyingOverWeightDetail
    {
        public Guid Id { get; set; }
        public Guid BuyingRouteId { get; set; }
        public decimal Ladder { get; set; }
        public short UnitId { get; set; }
        public decimal Price { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public decimal? ReferCustomerFee { get; set; }
    }
}
