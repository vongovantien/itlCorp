using System;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptQuickUpdateModel
    {
        public string PaymentMethod { get; set; }
        public string PaymentRefNo { get; set; }
        public Guid? OBHPartnerId { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
