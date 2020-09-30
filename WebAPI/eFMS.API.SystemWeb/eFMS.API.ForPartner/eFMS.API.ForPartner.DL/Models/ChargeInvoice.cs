using System;

namespace eFMS.API.ForPartner.DL.Models
{
    public class ChargeInvoice
    {
        public Guid ChargeID { get; set; }
        public Guid ChargeCode { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
