using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ExportSOADetailResult
    {
        public List<ExportSOAModel> ListCharges { get; set; }
        public string SOANo { get { return ListCharges[0].SOANo; } }
        public string CustomerName { get { return ListCharges[0].CustomerName; } }
        public string TaxCode { get { return ListCharges[0].TaxCode; } }
        public string CustomerAddress { get { return ListCharges[0].CustomerAddress; } }
        public string CurrencySOA { get { return ListCharges[0].CurrencySOA; } }
        public Nullable<decimal> TotalDebitExchange { get; set; }
        public Nullable<decimal> TotalCreditExchange { get; set; }
        public Nullable<decimal> TotalBalanceExchange { get { return TotalDebitExchange - TotalCreditExchange; } }
    }
}
