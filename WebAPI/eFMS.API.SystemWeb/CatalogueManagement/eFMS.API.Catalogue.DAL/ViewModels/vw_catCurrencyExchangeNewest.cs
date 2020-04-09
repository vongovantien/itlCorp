using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.Service.ViewModels
{
    public partial class vw_catCurrencyExchangeNewest
    {
        public int ID { get; set; }
        public string CurrencyFromID { get; set; }
        public decimal Rate { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string CurrencyToID { get; set; }
        public bool Active { get; set; }
    }
}
