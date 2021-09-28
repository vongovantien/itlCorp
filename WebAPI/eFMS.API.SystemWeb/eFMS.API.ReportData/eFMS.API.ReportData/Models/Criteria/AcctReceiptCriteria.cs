using System;


namespace eFMS.API.ReportData.Models.Criteria
{
    public class AcctReceiptCriteria
    {
        public string RefNo { get; set; }
        public string PaymentType { get; set; }
        public string CustomerID { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string DateType { get; set; }
        public string Currency { get; set; }
        public string SyncStatus { get; set; }
        public string Status { get; set; }
        public string TypeReceipt { get; set; }
    }
}
