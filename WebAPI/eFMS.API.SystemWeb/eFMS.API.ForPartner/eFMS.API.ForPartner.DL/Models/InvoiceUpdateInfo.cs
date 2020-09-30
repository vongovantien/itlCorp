using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Models
{
    public class InvoiceUpdateInfo
    {
        public string ReferenceNo { get; set; }
        public string PartnerCode { get; set; }
        public string OldInvoice { get; set; }
        public string NewInvoice { get; set; }
        public string SerieNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        List<ChargeInvoice> Charges { get; set; }

    }
}
