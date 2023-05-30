﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class AccAccountingManagementResult
    {
        public string JobNo { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
        public string VoucherId { get; set; } //VoucherId trên VAT Invoice Or Voucher
        public string CdNoteNo { get; set; }
        public string CdNoteType { get; set; }
        public string ChargeType { get; set; }
        public string PayerId { get; set; }
        public object PayerName { get; set; }
        public string PayerType { get; set; }
        public string Currency { get; set; }
        public string InvoiceNo { get; set; }
        public decimal? Amount { get; set; }
        public string IssueBy { get; set; }
        public string Bu { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime? VoucherIddate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string AccountNo { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ETD { get; set; }
        public string SoaNo { get; set; }
        public string ShipmentType { get; set; }
        public string AmountVND { get; set;}
        public decimal? ExchangeRate { get; set; }
        public string ParentAccountNo { get; set; }
    }
}
