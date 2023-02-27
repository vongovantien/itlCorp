using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsWorkOrderPrice
    {
        public Guid Id { get; set; }
        public Guid? PartnerId { get; set; }
        public decimal? UnitPriceBuying { get; set; }
        public decimal? UnitPriceSelling { get; set; }
        public decimal? VatrateBuying { get; set; }
        public decimal? VatrateSelling { get; set; }
        public string Notes { get; set; }
        public string CurrencyIdbuying { get; set; }
        public string CurrencyIdselling { get; set; }
        public decimal? WeightFromRange { get; set; }
        public decimal? WeightToRange { get; set; }
        public decimal? WeightFromValue { get; set; }
        public decimal? WeightToValue { get; set; }
        public short? UnitId { get; set; }
        public Guid? UserCreated { get; set; }
        public Guid? UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
