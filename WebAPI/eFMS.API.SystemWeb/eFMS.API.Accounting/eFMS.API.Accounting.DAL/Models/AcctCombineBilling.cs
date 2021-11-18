using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AcctCombineBilling
    {
        public Guid Id { get; set; }
        public string CombineBillingNo { get; set; }
        public string PartnerId { get; set; }
        public string Type { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAmountUsd { get; set; }
        public string Description { get; set; }
        public string Services { get; set; }
        public DateTime? IssuedDateFrom { get; set; }
        public DateTime? IssuedDateTo { get; set; }
        public DateTime? ServiceDateFrom { get; set; }
        public DateTime? ServiceDateTo { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
