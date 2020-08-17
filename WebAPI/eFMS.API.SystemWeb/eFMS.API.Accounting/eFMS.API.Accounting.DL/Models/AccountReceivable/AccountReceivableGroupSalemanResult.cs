using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class AccountReceivableGroupSalemanResult
    {
        public string SalesmanId { get; set; }
        public string SalesmanNameEn { get; set; }
        public string SalesmanFullName { get; set; }
        public decimal? TotalCreditLimited { get; set; }
        public decimal? TotalDebitAmount { get; set; }
        public decimal? TotalDebitRate { get; set; }
        public decimal? TotalBillingAmount { get; set; }
        public decimal? TotalBillingUnpaid { get; set; }
        public decimal? TotalPaidAmount { get; set; }
        public decimal? TotalObhAmount { get; set; }
        public decimal? TotalOver1To15Day { get; set; }
        public decimal? TotalOver16To30Day { get; set; }
        public decimal? TotalOver30Day { get; set; }
        public List<AccountReceivableResult> ArPartners { get; set; }
    }
}
