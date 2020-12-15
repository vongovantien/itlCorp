using System;

namespace eFMS.API.ForPartner.DL.Models
{
    public class VoucherAdvance
    {
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal? PaymentTerm { get; set; }
        public string AdvanceNo { get; set; }
        public Guid AdvanceID { get; set; }
    }

    public class RemoveVoucherAdvModel
    {
        public string VoucherNo { get; set; }
    }
}
