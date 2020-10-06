using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class CatCurrencyExchange
    {
        public int Id { get; set; }
        public string CurrencyFromId { get; set; }
        public string CurrencyToId { get; set; }
        public decimal Rate { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
