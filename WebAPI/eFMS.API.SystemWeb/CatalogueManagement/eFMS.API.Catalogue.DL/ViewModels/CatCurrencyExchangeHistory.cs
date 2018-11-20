using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatCurrencyExchangeHistory
    {
        public DateTime? DatetimeCreated { get; set; }
        public string LocalCurrency { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeUpdated{ get; set; }
    }
}
