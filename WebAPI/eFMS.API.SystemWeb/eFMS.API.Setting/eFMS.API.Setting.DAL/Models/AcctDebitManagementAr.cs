using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class AcctDebitManagementAr
    {
        public Guid Id { get; set; }
        public Guid? AcctManagementId { get; set; }
        public string Type { get; set; }
        public string PartnerId { get; set; }
        public string RefNo { get; set; }
        public Guid? Hblid { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAmountUsd { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? UnpaidAmountVnd { get; set; }
        public decimal? UnpaidAmountUsd { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public string PaymentStatus { get; set; }
        public Guid? CompanyId { get; set; }
        public string OfficeId { get; set; }
        public int? DepartmentId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
