using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.Service.ViewModels
{
    public class sp_GetDebitAmountDetailByContract
    {
        public string JobNo { get; set; }
        public string HBLNo { get; set; }
        public string MBLNo { get; set; }
        public string DebitNo { get; set; }
        public string SoaNo { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public decimal TotalVND { get; set; }
        public decimal TotalUSD { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime? ETD { get; set; }
        public DateTime? ETA { get; set; }
        public string TransactionType { get; set; }
        public string UserName { get; set; }
        public string PaymentStatus { get; set; }
        public string OfficeName { get; set; }
        public decimal PaidAmountVND { get; set; }
        public decimal PaidAmountUSD { get; set; }
    }
}
