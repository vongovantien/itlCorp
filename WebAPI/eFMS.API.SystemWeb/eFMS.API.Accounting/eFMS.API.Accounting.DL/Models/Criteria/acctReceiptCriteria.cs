using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AcctReceiptCriteria
    {
        public string PaymentRefNo { get; set; }
        public string PaymentType { get; set; }
        public string CustomerID { get; set; }
        public DateTime? Date { get; set; }
        public string Currency { get; set; }
        public string SyncStatus { get; set; }
    }
}
