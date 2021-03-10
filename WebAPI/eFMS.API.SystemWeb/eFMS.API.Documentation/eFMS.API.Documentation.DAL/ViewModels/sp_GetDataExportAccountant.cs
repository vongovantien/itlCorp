using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetDataExportAccountant
    {
        public string JobNo { get; set; }
        public string HWBNo { get; set; }
        public string Service { get; set; }
        public bool? KickBack { get; set; }
        public string InvoiceNo { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public decimal? VatAmountUsd { get; set; }
        public decimal? AmountUsd { get; set; }
        public decimal? AmountVnd { get; set; }
        public string CurrencyId { get; set; }
        public string VoucherId { get; set; }
        public string Type { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public string MAWB { get; set; }
        public Guid ChargeId { get; set; }
        public string CreditNo { get; set; }
        public string DebitNo { get; set; }
        public string PartnerId { get; set; }
        public decimal FinalExchangeRate { get; set; }
        public DateTime? ServiceDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public string ClearanceNo { get; set; }


    }
}
