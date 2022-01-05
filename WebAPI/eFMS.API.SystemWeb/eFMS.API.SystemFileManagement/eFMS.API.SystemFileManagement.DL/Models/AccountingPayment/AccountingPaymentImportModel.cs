﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.AccountingPayment
{
    public class AccountingPaymentImportModel
    {
        public bool IsValid { get; set; }
        public string InvoiceNo { get; set; }
        public string SOANo { get; set; }
        public string InvoiceNoError { get; set; }
        public string SerieNo { get; set; }
        public string SerieNoError { get; set; }
        public string PartnerAccount { get; set; }
        public string PartnerAccountError { get; set; }
        public string PartnerName { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentAmountError { get; set; }
        public DateTime PaidDate { get; set; }
        public string PaidDateError { get; set; }
        public string PaymentType { get; set; }
        public string PaymentTypeError { get; set; }
        public string Note { get; set; }
        public string PartnerId { get; set; }
        public string RefId { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyIdError { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodError { get; set; }
        public decimal ExchangeRate { get; set; }
        public string ExchangeRateError { get; set; }

    }
}
