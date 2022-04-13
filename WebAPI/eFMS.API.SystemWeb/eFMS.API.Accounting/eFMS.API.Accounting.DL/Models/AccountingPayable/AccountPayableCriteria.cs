using eFMS.API.Accounting.DL.Common;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AccountPayableCriteria
    {
        public string SearchType { get; set; }
        public string ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public string FromPaymentDate { get; set; }
        public string ToPaymentDate { get; set; }
        public List<string> PaymentStatus { get; set; }
        public List<string> TransactionType { get; set; }
        public List<string> Office { get; set; }
        public bool? IsPaging { get; set; }
    }
}
