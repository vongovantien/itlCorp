using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountPayable
{
    public class AcctPayableViewDetailCriteria
    {
        public string RefNo { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public string BillingNo { get; set; }
        public string BravoNo { get; set; }
    }
}
