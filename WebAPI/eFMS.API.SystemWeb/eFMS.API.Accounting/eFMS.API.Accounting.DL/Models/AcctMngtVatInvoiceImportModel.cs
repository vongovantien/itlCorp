using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctMngtVatInvoiceImportModel
    {
        public bool IsValid { get; set; }
        public string VoucherId { get; set; }
        public string RealInvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SerieNo { get; set; }
        public string PaymentStatus { get; set; }
    }
}
