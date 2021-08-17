using System;
using System.Collections.Generic;
using System.Text;

namespace FMS.API.ReportData.Models.Accounting
{
    public class AccountingAgencyPaymentExport
    {
        public string AgentPartnerCode { get; set; }
        public string AgentParentCode { get; set; }
        public string AgentPartnerName { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CreditNo { get; set; }
        public string JobNo { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string Salesman { get; set; }
        public string Creator { get; set; }

        public decimal? UnpaidAmountInv { get; set; }
        public decimal? UnpaidAmountOBH { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountOBH { get; set; }
        public DateTime? EtaDate { get; set; }
        public DateTime? EtdDate { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? PaymentCredit { get; set; }

        public List<AccountingAgencyPaymentExportDetail> details;
    }

    public class AccountingAgencyPaymentExportDetail
    {
        public string RefNo { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
    }
}
