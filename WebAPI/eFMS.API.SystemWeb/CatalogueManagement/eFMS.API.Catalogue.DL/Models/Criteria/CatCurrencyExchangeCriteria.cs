using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatCurrencyExchangeCriteria
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string LocalCurrencyId { get; set; }
    }
}
