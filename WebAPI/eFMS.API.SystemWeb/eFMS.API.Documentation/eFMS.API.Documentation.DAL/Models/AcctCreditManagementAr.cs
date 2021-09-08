using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class AcctCreditManagementAr
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string PartnerId { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hblno { get; set; }
        public Guid? Hblid { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? ExchangeRateUsdToLocal { get; set; }
        public string Currency { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
        public decimal? RemainVnd { get; set; }
        public decimal? RemainUsd { get; set; }
        public Guid? CompanyId { get; set; }
        public string OfficeId { get; set; }
        public int? DepartmentId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string SurchargeId { get; set; }
        public bool? NetOff { get; set; }
    }
}
