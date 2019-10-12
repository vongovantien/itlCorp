using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_catCurrencyExchange
    {
        public int Id { get; set; }
        public string CurrencyFromId { get; set; }
        public string CurrencyFromName { get; set; }
        public string CurrencyToId { get; set; }
        public string CurrencyToName { get; set; }
        public decimal Rate { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InActiveOn { get; set; }
    }
}
