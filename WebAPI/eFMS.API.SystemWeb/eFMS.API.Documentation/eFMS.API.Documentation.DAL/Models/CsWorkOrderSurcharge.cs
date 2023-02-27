using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsWorkOrderSurcharge
    {
        public Guid Id { get; set; }
        public Guid? ChargeId { get; set; }
        public string PartnerType { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Vatrate { get; set; }
        public byte[] CurrencyId { get; set; }
        public bool? KickBack { get; set; }
        public Guid? UserCreated { get; set; }
        public Guid? UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
