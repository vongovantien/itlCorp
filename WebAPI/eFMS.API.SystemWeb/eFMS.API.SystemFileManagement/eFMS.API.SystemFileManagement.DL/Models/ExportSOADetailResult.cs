using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class ExportSOADetailResult
    {
        public List<ExportSOAModel> ListCharges { get; set; }
        public string SOANo { get { return ListCharges.Count > 0 ? ListCharges[0].SOANo : null; } }
        public string CustomerName { get { return ListCharges.Count > 0 ? ListCharges[0].CustomerName : null; } }
        public string TaxCode { get { return ListCharges.Count > 0 ? ListCharges[0].TaxCode : null; } }
        public string CustomerAddress { get { return ListCharges.Count > 0 ? ListCharges[0].CustomerAddress : null; } }
        public string CurrencySOA { get { return ListCharges.Count > 0 ? ListCharges[0].CurrencySOA : null; } }
        public Nullable<decimal> TotalDebitExchange { get; set; }
        public Nullable<decimal> TotalCreditExchange { get; set; }
        public Nullable<decimal> TotalBalanceExchange { get { return TotalDebitExchange - TotalCreditExchange; } }
    }
}
