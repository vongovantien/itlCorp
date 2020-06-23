using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayment
{
    public class AccountingPaymentOBHImportTemplateModel
    {
        public string SoaNo { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int PaymentAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaymentType { get; set; }
        public bool isValid { get; set; }
    }
}
