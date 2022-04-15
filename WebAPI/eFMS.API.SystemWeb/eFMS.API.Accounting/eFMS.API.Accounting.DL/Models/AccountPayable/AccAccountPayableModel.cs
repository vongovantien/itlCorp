using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.AccountPayable
{
    public class AccAccountPayableModel
    {
        public string PaymentNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string CustomerCode { get; set; }
        public string OfficeCode { get; set; }
        public List<PayablePaymentDetailModel> PaymentDetail { get; set; }
    }

    public class PayablePaymentDetailModel
    {
        public string TransactionType { get; set; }
        public string BravoRefNo { get; set; }
        public string AdvRefNo { get; set; }
        public string Currency { get; set; }
        public decimal PayOriginAmount { get; set; }
        public decimal PayAmountVND { get; set; }
        public decimal PayAmountUSD { get; set; }
        public decimal RemainOriginAmount { get; set; }
        public decimal RemainAmountVND { get; set; }
        public decimal RemainAmountUSD { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
