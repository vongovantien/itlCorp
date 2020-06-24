using eFMS.API.Accounting.DL.Common;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class PaymentCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public DateTime? FromIssuedDate { get; set; }
        public DateTime? ToIssuedDate { get; set; }
        public DateTime? FromUpdatedDate { get; set; }
        public DateTime? ToUpdatedDate { get; set; }
        public DateTime? FromDueDate { get; set; }
        public DateTime? ToDueDate { get; set; }
        public OverDueDate OverDueDays { get; set; }
        public string PaymentStatus { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
