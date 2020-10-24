using System;

namespace eFMS.API.ForPartner.DL.Models
{
    public class ChargeInvoice
    {
        public Guid ChargeId { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeType { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string ReferenceNo { get; set; }
        public decimal? PaymentTerm { get; set; }
    }
}
