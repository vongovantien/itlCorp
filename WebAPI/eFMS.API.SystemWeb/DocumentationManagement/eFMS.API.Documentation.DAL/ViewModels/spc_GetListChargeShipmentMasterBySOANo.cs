using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class spc_GetListChargeShipmentMasterBySOANo
    {
        public Guid ID { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime ServiceDate { get; set; }
        public string Note { get; set; }
        public string Currency { get; set; }
        public string CurrencyToLocal { get; set; }
        public string CurrencyToUSD { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public decimal AmountDebitLocal { get; set; }
        public decimal AmountCreditLocal { get; set; }
        public decimal AmountDebitUSD { get; set; }
        public decimal AmountCreditUSD { get; set; }
    }
}
