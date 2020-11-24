using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ConfirmBillingCriteria
    {
        public string SearchOption { get; set; }
        public List<string> ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public string DateType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsConfirmedBilling { get; set; }
        public string Services { get; set; }
        public string CsHandling { get; set; }
    }
}
