using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CatContract
    {
        public Guid Id { get; set; }
        public string SaleManId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string SaleService { get; set; }
        public string PartnerId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public string PaymentMethod { get; set; }
        public string ContractNo { get; set; }
        public string ContractType { get; set; }
        public string Vas { get; set; }
        public decimal? TrialCreditLimited { get; set; }
        public int? TrialCreditDays { get; set; }
        public DateTime? TrialEffectDate { get; set; }
        public DateTime? TrialExpiredDate { get; set; }
        public int? PaymentTerm { get; set; }
        public decimal? CreditLimit { get; set; }
        public int? CreditLimitRate { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? BillingAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? CustomerAdvanceAmount { get; set; }
        public int? CreditRate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
