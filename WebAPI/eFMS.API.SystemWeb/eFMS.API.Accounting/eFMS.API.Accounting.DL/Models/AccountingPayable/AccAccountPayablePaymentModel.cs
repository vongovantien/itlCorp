using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayable
{
    public class AccAccountPayablePaymentModel : AccAccountPayablePayment
    {
        public string RefNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string DocNo { get; set; }
    }
}
