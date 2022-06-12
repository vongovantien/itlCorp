using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AcctReceivableDebitDetailCriteria
    {
        public string PartnerId { get; set; }
        public string OfficeId { get; set; }
        public string Service { get; set; }
        public string Type { get; set; }
        public string PaymentStatus { get; set; }
        public string Salesman { get; set; }
        public string OverDue { get; set; }
    }
}
