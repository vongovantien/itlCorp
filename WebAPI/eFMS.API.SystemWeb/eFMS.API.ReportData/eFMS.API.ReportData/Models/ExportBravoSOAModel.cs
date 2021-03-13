using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class ExportBravoSOAModel
    {
        public string SOANo { get; set; }
        public string TaxCode { get; set; }
        public string PartnerCode { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string Service { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string CreditDebitNo { get; set; }
        public string TransationType { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string OriginalCurrency { get; set; }
        public decimal? OriginalAmount { get; set; }
        public decimal? AmountVND { get; set; }
        public decimal? VAT { get; set; }
        public string AccountDebitNoVAT { get; set; }
        public string AccountCreditNoVAT { get; set; }
        public decimal? AmountVAT { get; set; }
        public decimal? AmountVNDVAT { get; set; }
        public string Commodity { get; set; }
        public Nullable<decimal> DebitExchange { get; set; }
        public Nullable<decimal> CreditExchange { get; set; }
        public string Unit { get; set; }
        public string Payment { get; set; }
        public decimal? Quantity { get; set; }
        public string Email { get; set; }
        public string TaxCodeOBH { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SeriesNo { get; set; }
    }
}
