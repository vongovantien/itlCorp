using System;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class DebitAmountDetailbyPartnerIdModel
    {
        public string JobNo { get; set; }
        public string HBLNo { get; set; }
        public string MBLNo { get; set; }
        public string DebitNo { get; set; }
        public string SoaNo { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public double TotalVND { get; set; }
        public double TotalUSD { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime? ETD { get; set; }
        public DateTime? ETA { get; set; }
        public string TransactionType { get; set; }
        public string UserName { get; set; }
        public string PaymentStatus { get; set; }
        public string OfficeName { get; set; }
        public double PaidAmountVND { get; set; }
        public double PaidAmountUSD { get; set; }
    }
}
