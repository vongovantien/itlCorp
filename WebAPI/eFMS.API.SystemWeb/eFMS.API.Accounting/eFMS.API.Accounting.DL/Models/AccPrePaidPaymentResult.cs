using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccPrePaidPaymentResult
    {
        public Guid Id { get; set; }
        public string JobNo { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string DebitNote { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountVND { get; set; }
        public decimal TotalAmountUSD { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountVND { get; set; }
        public decimal PaidAmountUSD { get; set; }
        public string Currency { get; set; }
        public string SalesmanName { get; set; }
        public string Status { get; set; }
    }

    public class AccountingPrePaidPaymentCriteria
    {
        public string SearchType { get; set; }
        public List<string> Keywords { get; set; }
        public string PartnerId { get; set; }
        public string SalesmanId { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string AgreementType { get; set; }

    }
}
