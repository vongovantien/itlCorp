using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class PreviewCdNoteCriteria
    {
        public Guid JobId { get; set; }
        public string CreditDebitNo { get; set; }
        public string Currency { get; set; }
    }
}
