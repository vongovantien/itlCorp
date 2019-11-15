using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class PreviewCdNoteCriteria
    {
        public Guid JobId { get; set; }
        public string CreditDebitNo { get; set; }
        public string Currency { get; set; }
    }
}
