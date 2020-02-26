using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatPartnerCharge
    {
        public Guid Id { get; set; }
        public string PartnerId { get; set; }
        public Guid ChargeId { get; set; }
        public decimal? Quantity { get; set; }
        public string QuantityType { get; set; }
        public short? UnitId { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Vatrate { get; set; }
    }
}
