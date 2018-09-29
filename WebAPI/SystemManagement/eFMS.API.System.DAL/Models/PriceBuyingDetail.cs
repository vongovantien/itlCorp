﻿using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceBuyingDetail
    {
        public Guid Id { get; set; }
        public Guid BuyingRouteId { get; set; }
        public decimal FromValue { get; set; }
        public decimal ToValue { get; set; }
        public short UnitId { get; set; }
        public decimal Price { get; set; }
        public bool? IsMinCharges { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public decimal? ReferCustomerFee { get; set; }
    }
}
