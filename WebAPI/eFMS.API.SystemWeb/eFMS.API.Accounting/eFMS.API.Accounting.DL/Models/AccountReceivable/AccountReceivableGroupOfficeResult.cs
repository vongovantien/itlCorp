using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class AccountReceivableGroupOfficeResult
    {
        public Guid? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public decimal? TotalDebitAmount { get; set; }
        public decimal? TotalBillingAmount { get; set; }
        public decimal? TotalBillingUnpaid { get; set; }
        public decimal? TotalPaidAmount { get; set; }
        public decimal? TotalObhAmount { get; set; }
        public decimal? TotalOver1To15Day { get; set; }
        public decimal? TotalOver15To30Day { get; set; }
        public decimal? TotalOver30Day { get; set; }
        public List<AccountReceivableServiceResult> AccountReceivableGrpServices { get; set; }
    }
}
