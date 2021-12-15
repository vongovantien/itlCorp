﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AccAccountPayable
    {
        public Guid Id { get; set; }
        public string PartnerId { get; set; }
        public Guid? VoucherId { get; set; }
        public string ReferenceNo { get; set; }
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public DateTime? BillingDate { get; set; }
        public string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAmountUsd { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? PaymentAmountVnd { get; set; }
        public decimal? PaymentAmountUsd { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? UnpaidAmountVnd { get; set; }
        public decimal? UnpaidAmountUsd { get; set; }
        public decimal? RemainAmount { get; set; }
        public decimal? RemainAmountVnd { get; set; }
        public decimal? RemainAmountUsd { get; set; }
        public string Status { get; set; }
        public string TransactionType { get; set; }
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
