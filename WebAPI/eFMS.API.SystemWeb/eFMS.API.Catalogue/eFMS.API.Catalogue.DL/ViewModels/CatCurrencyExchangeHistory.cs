using System;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatCurrencyExchangeHistory
    {
        public DateTime? DatetimeCreated { get; set; }
        public string LocalCurrency { get; set; }
        public string UserModifield { get; set; }
        public DateTime? DatetimeUpdated{ get; set; }
    }
}
