using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AcctAdvanceRequest
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string CustomNo { get; set; }
        public string JobId { get; set; }
        public string Hbl { get; set; }
        public string Mbl { get; set; }
        public Guid? Hblid { get; set; }
        public decimal? Amount { get; set; }
        public string RequestCurrency { get; set; }
        public string AdvanceType { get; set; }
        public string RequestNote { get; set; }
        public string StatusPayment { get; set; }
        public string AdvanceNo { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
