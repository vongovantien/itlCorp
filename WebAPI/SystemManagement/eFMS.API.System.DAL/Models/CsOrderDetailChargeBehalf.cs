using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderDetailChargeBehalf
    {
        public Guid Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public string SupplierId { get; set; }
        public Guid? TransportRequestId { get; set; }
        public string ChargeId { get; set; }
        public decimal? Price { get; set; }
        public string CurrencyId { get; set; }
        public string BehalfType { get; set; }
        public string InvoiceNo { get; set; }
        public string Notes { get; set; }
        public bool? IncludedVat { get; set; }
        public bool? CheckRevenue { get; set; }
        public string ObjectBePaid { get; set; }
        public string PaymentRefNo { get; set; }
        public decimal? Remain { get; set; }
        public bool? IsAddition { get; set; }
        public string AccountantId { get; set; }
        public DateTime? AccountantDate { get; set; }
        public string AccountantStatus { get; set; }
        public string AccountantNote { get; set; }
        public string ChiefAccountantId { get; set; }
        public DateTime? ChiefDate { get; set; }
        public string ChiefStatus { get; set; }
        public string ChiefNote { get; set; }
        public string OpsmanId { get; set; }
        public DateTime? OpsmanDate { get; set; }
        public string OpsmanStatus { get; set; }
        public string Status { get; set; }
        public string OpsmanNote { get; set; }
        public string CsidtripSettlement { get; set; }
        public DateTime? CsdateTripSettlement { get; set; }
        public string CsstatusTripSettlement { get; set; }
        public string TripSettlementCode { get; set; }
        public string ReSignedUser { get; set; }
        public DateTime? ReSignedDate { get; set; }
        public string ReceivingPlace { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
