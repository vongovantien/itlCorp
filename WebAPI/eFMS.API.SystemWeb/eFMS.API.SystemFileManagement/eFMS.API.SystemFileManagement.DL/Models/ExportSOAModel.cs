using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class ExportSOAModel
    {
        public string SOANo { get; set; }
        public string CustomerName { get; set; }
        public string TaxCode { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string CreditDebitNo { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public string CurrencySOA { get; set; }
        public string CurrencyCharge { get; set; }
        public Nullable<decimal> DebitExchange { get; set; }
        public Nullable<decimal> CreditExchange { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public DateTime? ExchangeDate { get; set; }
    }
}
