using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class CatCurrency
    {
        public string Id { get; set; }
        public string CurrencyName { get; set; }
        public bool IsDefault { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
