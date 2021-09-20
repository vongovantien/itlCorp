﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class AccountingAgencyPaymentExport
    {
        public string AgentPartnerCode { get; set; }
        public string AgentParentCode { get; set; }
        public string AgentPartnerName { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CreditNo { get; set; }
        public DateTime? BillingDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? UnpaidAmountInv { get; set; }
        public decimal? UnpaidAmountOBH { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountOBH { get; set; }
        public string JobNo { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string Salesman { get; set; }
        public string Creator { get; set; }
        public string RefNo { get; set; }
        public string PaymentNo { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? EtaDate { get; set; }
        public DateTime? EtdDate { get; set; }
        public decimal CreditAmount { get;  set; }
        public decimal PaymentCredit { get; set; }
        public decimal? NetOff { get;  set; }
        public string DebitNo { get; set; }
        public decimal? CreditTerm { get; set; }
        public int? OverDueDays { get; set; }
        public decimal? DebitAmount { get; set; }
        public int? OverdueDays { get; set; }
        public decimal? RemainDbUsd { get; set; }
        public decimal? RemainOBHUsd { get; set; }
        public decimal? UnpaidAmountInvUsd { get; set; }
        public decimal? PaidAmountOBHUsd { get; set; }
        public decimal? UnpaidAmountOBHUsd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public string VoucherNo { get;  set; }

        public List<AccountingAgencyPaymentExportDetail> details;
    }

    public class AccountingAgencyPaymentExportDetail
    {
        public string RefNo { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? PaidAmountUsd { get;  set; }
        public decimal? PaidAmountOBHUsd { get;  set; }
        public decimal? PaidAmount { get; internal set; }
        public decimal? PaidAmountOBH { get; internal set; }
    }
}
