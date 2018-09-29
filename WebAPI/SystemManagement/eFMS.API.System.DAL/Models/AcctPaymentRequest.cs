using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AcctPaymentRequest
    {
        public Guid Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string RefNo { get; set; }
        public string PaymentObject { get; set; }
        public int? RequestedDriver { get; set; }
        public string OtherRequestor { get; set; }
        public string PaymentMethodType { get; set; }
        public string Beneficiary { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string Status { get; set; }
        public string OpsmanId { get; set; }
        public string OpsmanStatus { get; set; }
        public DateTime? OpsmanDate { get; set; }
        public string AccountantId { get; set; }
        public string AccountantStatus { get; set; }
        public DateTime? AccountantDate { get; set; }
        public string ChiefAccountantId { get; set; }
        public string ChiefAccountantStatus { get; set; }
        public DateTime? ChiefAccountantDate { get; set; }
        public string DirectorId { get; set; }
        public string DirectorStatus { get; set; }
        public DateTime? DirectorDate { get; set; }
        public bool? CheckPayment { get; set; }
        public string UserCheckPayment { get; set; }
        public DateTime? DatetimeCheckPayment { get; set; }
        public decimal? TotalPayment { get; set; }
        public decimal? NotPaidPriceTotal { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string BankAddress { get; set; }
        public string Type { get; set; }
    }
}
