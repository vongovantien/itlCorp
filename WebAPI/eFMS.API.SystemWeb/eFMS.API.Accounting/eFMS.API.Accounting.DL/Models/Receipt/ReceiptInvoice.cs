﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptInvoiceModel: CustomerDebitCreditModel
    {
        public Guid? PaymentId { get; set; }
        public string Notes { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
    }

    public class ProcessClearInvoiceModel
    {
        public List<ReceiptInvoiceModel> Invoices { get; set; }
        public decimal Balance { get; set; }
    }
}
