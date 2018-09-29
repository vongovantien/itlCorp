using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsChangedSurchargeLog
    {
        public int Id { get; set; }
        public Guid TransportSurchargeId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string OldCurrencyId { get; set; }
        public string NewCurrencyId { get; set; }
        public bool? OldIncludedVat { get; set; }
        public bool? NewIncludedVat { get; set; }
        public string OldObjectBePaid { get; set; }
        public string NewObjectBePaid { get; set; }
        public string OldPaymentObjectId { get; set; }
        public string NewPaymentObjectId { get; set; }
        public string ChargeType { get; set; }
        public bool? RequestedAgain { get; set; }
        public string Note { get; set; }
        public string UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
