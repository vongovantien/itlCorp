using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderDetailSurcharge
    {
        public Guid Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public string ChargeId { get; set; }
        public Guid? TransportRequestId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public bool? IncludedVat { get; set; }
        public string Notes { get; set; }
        public bool? CheckRevenue { get; set; }
        public string InvoiceNo { get; set; }
        public string Status { get; set; }
        public bool? Collected { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
