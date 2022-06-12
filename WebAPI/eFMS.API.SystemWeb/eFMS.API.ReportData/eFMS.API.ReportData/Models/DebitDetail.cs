﻿using System;

namespace eFMS.API.ReportData.Models
{
    public class DebitDetail
    {
        public string BillingNo { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalAmountVND { get; set; }
        public decimal TotalAmountUSD { get; set; }
        public decimal PaidAmountVND { get; set; }
        public decimal PaidAmountUSD { get; set; }
        public decimal UnpaidAmountVND { get; set; }
        public decimal UnpaidAmountUSD { get; set; }
        public int OverdueDays { get; set; }
        public string Code { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public string PaymentStatus { get; set; }
        public string Service { get; set; }
        public string Salesman { get; set; }
    }
}
