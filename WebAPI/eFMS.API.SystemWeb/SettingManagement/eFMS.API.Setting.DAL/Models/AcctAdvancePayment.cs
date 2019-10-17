using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class AcctAdvancePayment
    {
        public Guid Id { get; set; }
        public string AdvanceNo { get; set; }
        public string Requester { get; set; }
        public string Department { get; set; }
        public string PaymentMethod { get; set; }
        public string AdvanceCurrency { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DeadlinePayment { get; set; }
        public string StatusApproval { get; set; }
        public string AdvanceNote { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
