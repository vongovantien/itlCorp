using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptInvoiceCriteria
    {
        public string CustomerID { get; set; }
        public string AgreementID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
