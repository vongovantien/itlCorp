using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Models
{
    public class InvoiceData
    {
        public string InvocieNo { get; set; }
        public string InvoiceDate { get; set; }
        public string ReferenceNo { get; set; }
        public string PartnerCode { get; set; }
        public string SerieNo { get; set; }
        public List<ChargeInvoice> Charges { get; set; }

    }
}
