using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsWorkOrderSurcharge
    {
        public Guid Id { get; set; }
        public Guid WorkOrderPriceId { get; set; }
        public Guid WorkOrderId { get; set; }
        public Guid? ChargeId { get; set; }
        public string PartnerId { get; set; }
        public string Type { get; set; }
        public string PartnerType { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Vatrate { get; set; }
        public byte[] CurrencyId { get; set; }
        public bool? KickBack { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
