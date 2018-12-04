using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_catCurrencyExchangeNewest
    {
        public string CurrencyFromId { get; set; }
        public decimal Rate { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
