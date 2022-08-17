using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayable
{
    public class GeneralAccPayableModel
    {
        public decimal? PaymentTerm { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? CreditPaidAmount { get; set; }
        public decimal? CreditUnpaidAmount { get; set; }
        public decimal? CreditAdvanceAmount { get; set; }
        public string Currency { get; set; }
    }
}
