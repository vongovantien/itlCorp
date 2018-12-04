using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class CsShipmentProfitShares
    {
        public string Hawbno { get; set; }
        public string PartnerId { get; set; }
        public string ChagreFeeId { get; set; }
        public double Qty { get; set; }
        public string Qunit { get; set; }
        public string CurrUnit { get; set; }
        public double? UnitPrice { get; set; }
        public string CurrencyConvertRate { get; set; }
        public double? Vat { get; set; }
        public double? TotalValue { get; set; }
        public double? ExRateSaleVnd { get; set; }
        public double? ExRateInvoiceVnd { get; set; }
        public double? AmountNoVatvnd { get; set; }
        public double? AmountVatvnd { get; set; }
        public double? AmountNoVatusd { get; set; }
        public double? AmountVatusd { get; set; }
        public bool Collect { get; set; }
        public string Notes { get; set; }
        public bool Debit { get; set; }
        public bool? Obh { get; set; }
        public string ObhpartnerId { get; set; }
        public bool Paid { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Docs { get; set; }
        public string SeriNo { get; set; }
        public string Vatname { get; set; }
        public string VattaxCode { get; set; }
        public string VoucherId { get; set; }
        public string VoucherIdse { get; set; }
        public string SettlementRefNo { get; set; }
        public string InoiceNo { get; set; }
        public string VatinvId { get; set; }
        public string Soano { get; set; }
        public string UserCreated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string AcctantCreated { get; set; }
        public string AcctantDate { get; set; }
        public bool? ShipmentLockedStatus { get; set; }
        public DateTime? ShipmentLockedDate { get; set; }
    }
}
