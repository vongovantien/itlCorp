using System;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class DebitDetailCriteria
    {
        public string PartnerId { get; set; }
        public string OfficeId { get; set; }
        public string Service { get; set; }
        public string Type { get; set; }
        public string PaymentStatus { get; set; }
        public string Salesman { get; set; }
        public string OverDue { get; set; }
    }
}
