using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsWorkOrderPrice
    {
        public Guid Id { get; set; }
        public Guid WorkOrderId { get; set; }
        public string Type { get; set; }
        public Guid? PartnerId { get; set; }
        public Guid? ChargeIdBuying { get; set; }
        public Guid? ChargeIdSelling { get; set; }
        public decimal? UnitPriceBuying { get; set; }
        public decimal? UnitPriceSelling { get; set; }
        public decimal? VatrateBuying { get; set; }
        public decimal? VatrateSelling { get; set; }
        public string Notes { get; set; }
        public string CurrencyIdBuying { get; set; }
        public string CurrencyIdSelling { get; set; }
        public decimal? QuantityFromValue { get; set; }
        public decimal? QuantityToValue { get; set; }
        public short? UnitId { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string QuantityType { get; set; }
    }
}
