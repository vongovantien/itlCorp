using System;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class CustomerDebitCreditCriteria
    {
        public string PartnerId { get; set; }
        public string SearchType { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string DateType { get; set; }
        public string Service { get; set; }
    }
}
