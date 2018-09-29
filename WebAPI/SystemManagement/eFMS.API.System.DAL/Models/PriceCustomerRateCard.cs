using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceCustomerRateCard
    {
        public string CustomerId { get; set; }
        public int RateCardId { get; set; }
        public bool? CustomerAccepted { get; set; }
        public int? Contract { get; set; }
    }
}
