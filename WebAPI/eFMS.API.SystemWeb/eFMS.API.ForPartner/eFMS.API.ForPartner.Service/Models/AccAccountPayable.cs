﻿using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class AccAccountPayable
    {
        public Guid Id { get; set; }
        public Guid? PartnerId { get; set; }
        public Guid? VoucherId { get; set; }
        public string PaymentNo { get; set; }
        public byte[] ReferenceNo { get; set; }
        public string PaymentType { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Currency { get; set; }
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
        public byte[] Status { get; set; }
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
