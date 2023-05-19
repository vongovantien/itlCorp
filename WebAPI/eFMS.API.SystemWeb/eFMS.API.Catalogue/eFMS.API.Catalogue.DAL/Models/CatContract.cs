﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatContract
    {
        public Guid Id { get; set; }
        public string SaleManId { get; set; }
        public string OfficeId { get; set; }
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
        public string BaseOn { get; set; }
        public bool? Arconfirmed { get; set; }
        public decimal? CreditLimit { get; set; }
        public int? CreditLimitRate { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? BillingAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? CreditRate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string CurrencyId { get; set; }
        public bool? CreditUnlimited { get; set; }
        public string CreditCurrency { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? AutoExtendDays { get; set; }
        public decimal? CustomerAdvanceAmountVnd { get; set; }
        public decimal? CustomerAdvanceAmountUsd { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsOverLimit { get; set; }
        public bool? IsOverDue { get; set; }
        public short? SalesGroup { get; set; }
        public int? SalesDepartment { get; set; }
        public string SalesOfficeId { get; set; }
        public string SalesCompanyId { get; set; }
        public bool? NoDue { get; set; }
        public string ShipmentType { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? FirstShipmentDate { get; set; }
        public int? PaymentTermObh { get; set; }
        public bool? IsOverDueObh { get; set; }
        public bool? IsOverDuePrepaid { get; set; }
    }
}
