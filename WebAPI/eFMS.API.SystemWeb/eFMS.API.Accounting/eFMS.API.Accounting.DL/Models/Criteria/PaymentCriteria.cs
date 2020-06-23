using eFMS.API.Accounting.DL.Common;
using System;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class PaymentCriteria
    {
        public string RefNos { get; set; }
        public string PartnerId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public OverDueDate OverDueDays { get; set; }
        public string PaymentStatus { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
