using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class AccAccountPayablePayment
    {
        public Guid Id { get; set; }
        public string PartnerId { get; set; }
        public string PaymentNo { get; set; }
        public string ReferenceNo { get; set; }
        public string PaymentType { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Currency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Status { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? PaymentAmountVnd { get; set; }
        public decimal? PaymentAmountUsd { get; set; }
        public decimal? RemainAmount { get; set; }
        public decimal? RemainAmountVnd { get; set; }
        public decimal? RemainAmountUsd { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string AcctId { get; set; }
    }
}
