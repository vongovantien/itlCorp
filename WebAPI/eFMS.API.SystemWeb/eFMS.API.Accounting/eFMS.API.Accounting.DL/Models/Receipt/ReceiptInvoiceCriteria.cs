using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptInvoiceCriteria
    {
        public Guid CustomerID { get; set; }
        public Guid AgreementID { get; set; }
        public DateTime? Date { get; set; }
        public string ReceiptNo { get; set; }
    }
}
