using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsShipmentSurcharge
    {
        public Guid Id { get; set; }
        public Guid Hblid { get; set; }
        public string Type { get; set; }
        public Guid ChargeId { get; set; }
        public decimal Quantity { get; set; }
        public short UnitId { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public bool? IncludedVat { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal Total { get; set; }
        public string PayerId { get; set; }
        public string ReceiverObject { get; set; }
        public string ReceiverId { get; set; }
        public string ObjectBePaid { get; set; }
        public string PaymentObjectId { get; set; }
        public bool? KickBack { get; set; }
        public DateTime? ExchangeDate { get; set; }
        public string Notes { get; set; }
        public string DocNo { get; set; }
        public string SettlementCode { get; set; }
        public string Csidsettlement { get; set; }
        public string CsstatusSettlement { get; set; }
        public DateTime? CsdateSettlement { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentRefNo { get; set; }
        public string AccountantId { get; set; }
        public DateTime? AccountantDate { get; set; }
        public string AccountantStatus { get; set; }
        public string AccountantNote { get; set; }
        public string ChiefAccountantId { get; set; }
        public DateTime? ChiefAccountantDate { get; set; }
        public string ChiefAccountantStatus { get; set; }
        public string ChiefAccountantNote { get; set; }
        public string Status { get; set; }
        public string SoaadjustmentRequestor { get; set; }
        public DateTime? SoaadjustmentRequestedDate { get; set; }
        public string SoaadjustmentReason { get; set; }
        public string UnlockedSoasaleMan { get; set; }
        public string UnlockedSoasaleManStatus { get; set; }
        public DateTime? UnlockedSoasaleManDate { get; set; }
        public string UnlockedSoadirector { get; set; }
        public string UnlockedSoadirectorStatus { get; set; }
        public DateTime? UnlockedSoadirectorDate { get; set; }
        public bool? Soaclosed { get; set; }
        public string Soano { get; set; }
        public string OtherSoa { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
