using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AccAccountingPayment
    {
        public Guid Id { get; set; }
        public string RefNo { get; set; }
        public string PaymentNo { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? Balance { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaymentType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
