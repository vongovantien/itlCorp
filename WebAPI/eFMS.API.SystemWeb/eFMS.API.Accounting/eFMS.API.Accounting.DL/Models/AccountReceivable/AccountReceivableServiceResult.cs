using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class AccountReceivableServiceResult
    {
        public Guid? OfficeId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? BillingAmount { get; set; }
        public decimal? BillingUnpaid { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? ObhAmount { get; set; }
        public decimal? Over1To15Day { get; set; }
        public decimal? Over16To30Day { get; set; }
        public decimal? Over30Day { get; set; }
        public string Currency { get; set; }
        public decimal? ObhBillingAmount { get; set; }
        public decimal? ObhPaidAmount { get; set; }
        public decimal? ObhUnPaidAmount { get; set; }
    }
}
