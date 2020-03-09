using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class DetailSOAModel
    {
        public List<SOAModel> ListCharges { get; set; }
        public string SOANo { get; set; }
        public string CustomerName { get; set; }
        public string TaxCode { get; set; }
        public string CustomerAddress { get; set; }
        public string CurrencySOA { get; set; }
        public Nullable<decimal> TotalDebitExchange { get; set; }
        public Nullable<decimal> TotalCreditExchange { get; set; }
        public Nullable<decimal> TotalBalanceExchange { get; set; }
    }
}
