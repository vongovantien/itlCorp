using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayable
{
    public class AccountingTemplateExport
    {
        public string OfficeName { get; set; }
        public string ContactOffice { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string Code { get; set; }
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string DocNo { get; set; }
        public string AccountNo { get; set; }
        public string Description { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public decimal? BeginAmount { get; set; }
        public decimal? OrgCreditAmount { get; set; }
        public decimal? OrgAdvAmount { get; set; }
        public decimal? BeginAmountVND { get; set; }
        public decimal? OrgCreditAmountVND { get; set; }
        public decimal? OrgAdvAmountVND { get; set; }
        
    }
}
