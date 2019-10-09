using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class AcctSettlementPayment
    {
        public Guid Id { get; set; }
        public string SettlementNo { get; set; }
        public string Requester { get; set; }
        public DateTime? RequestDate { get; set; }
        public string PaymentMethod { get; set; }
        public string SettlementCurrency { get; set; }
        public string StatusApproval { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
