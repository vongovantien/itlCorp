using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CombineBillingCriteria
    {
        public string CdNoteCode { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string CurrencyCombine { get; set; }
    }
}
