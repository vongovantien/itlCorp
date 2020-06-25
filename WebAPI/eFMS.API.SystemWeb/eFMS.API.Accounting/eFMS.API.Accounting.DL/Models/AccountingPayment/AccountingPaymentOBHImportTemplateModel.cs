﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayment
{
    public class AccountingPaymentOBHImportTemplateModel
    {
        public string SoaNo { get; set; }
        public string SoaNoError { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int? PaymentAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaymentType { get; set; }
        public bool IsValid { get; set; }
        public string PartnerAccountError { get; set; }
        public string RefId { get; set; }
        public string PaidDateError { get; set; }
    }
}
