using eFMS.API.Catalogue.Service.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CurrencyExchangeNewestViewModel
    {
        public DateTime DatetimeCreated { get; set; }
        public string UserModifield { get; set; }
        public string LocalCurrency { get; set; }
        public List<vw_catCurrencyExchangeNewest> ExchangeRate { get; set; }
    }
}
