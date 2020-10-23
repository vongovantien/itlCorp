using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Models
{
    public class InvoiceUpdateInfo
    {
        public string ReferenceNo { get; set; }
        public string PreReferenceNo { get; set; }
        public string PartnerCode { get; set; }
        public string InvoiceNo { get; set; }
        public string SerieNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Currency { get; set; }
        public decimal? PaymentTerm { get; set; }
        public List<ChargeInvoice> Charges { get; set; }
    }
}
