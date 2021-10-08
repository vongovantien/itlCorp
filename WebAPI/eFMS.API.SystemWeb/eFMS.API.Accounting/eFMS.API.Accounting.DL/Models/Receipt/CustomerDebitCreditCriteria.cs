using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class CustomerDebitCreditCriteria
    {
        public string PartnerId { get; set; }
        public string SearchType { get; set; }
        public List<string> ReferenceNos { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string DateType { get; set; }
        public List<string> Service { get; set; }
        public List<string> Office { get; set; }
    }
}
