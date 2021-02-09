using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class DataSurchargeResult
    {
        public Guid? Hblid { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string JobId { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public string  Type { get; set; }
        public string PaymentObjectId { get; set; }
        public string InvoiceNo { get; set; }
   
        public string CreditNo { get; set; }
        public string DebitNo { get; set; }
       
        public string VoucherId { get; set; }
        public bool? KickBack { get; set; }
        public string AdvanceNo { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hblno { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
        public decimal? VatAmountUsd { get; set; }
        public Guid ChargeId { get; set; }
        public string Service { get; set; }
        public string ClearanceNo { get; set; }
    }
}
