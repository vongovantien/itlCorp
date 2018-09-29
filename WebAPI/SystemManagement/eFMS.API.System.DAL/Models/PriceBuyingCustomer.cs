﻿using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceBuyingCustomer
    {
        public Guid Id { get; set; }
        public Guid BuyingId { get; set; }
        public string CustomerId { get; set; }
        public string BuyingType { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
