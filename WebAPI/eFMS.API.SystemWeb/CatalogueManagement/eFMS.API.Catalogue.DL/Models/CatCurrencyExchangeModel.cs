using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatCurrencyExchangeModel: CatCurrencyExchange
    {
    }
    public class CatCurrencyExchangeEditModel
    {
        public string CurrencyToId { get; set; }
        public List<CatCurrencyExchangeRate> CatCurrencyExchangeRates { get; set; }
        public string UserModified { get; set; }

    }
    public class CatCurrencyExchangeRate
    {
        public string CurrencyFromId { get; set; }
        public decimal Rate { get; set; }
    }
}
