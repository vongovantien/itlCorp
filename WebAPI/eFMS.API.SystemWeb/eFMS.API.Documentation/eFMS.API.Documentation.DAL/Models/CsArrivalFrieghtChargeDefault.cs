using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsArrivalFrieghtChargeDefault
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public string Description { get; set; }
        public Guid? ChargeId { get; set; }
        public decimal? Quantity { get; set; }
        public short? UnitId { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal? Total { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Notes { get; set; }
        public bool? IsShow { get; set; }
        public bool? IsFull { get; set; }
        public bool? IsTick { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
