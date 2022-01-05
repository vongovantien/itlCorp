using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.ExportResults
{
    public class SettlementExportDefault
    {
        public string JobID { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string CustomNo { get; set; }
        public string SettleNo { get; set; }
        public string Requester { get; set; }
        public decimal? SettlementAmount { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public string Currency { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Description { get; set; }
    }

    public class ShipmentSettlementExportGroup
    {
        public string SettleNo { get; set; }
        public string JobID { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string CustomNo { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Code { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public decimal Quantity { get; set; }
        public short UnitId { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public string ChargeUnit { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAmountUsd { get; set; }
        public string Payee { get; set; }
        public string OBHPartnerName { get; set; }
        public string InvoiceNo { get; set; }
        public string SeriesNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string VatPartner { get; set; }
        public string AdvanceNo { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? Balance { get; set; }
        public List<SurchargesShipmentSettlementExportGroup> surchargesDetail;
    }

    public class SurchargesShipmentSettlementExportGroup
    {
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public string ChargeUnit { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public string Payee { get; set; }
        public string OBHPartnerName { get; set; }
        public string InvoiceNo { get; set; }
        public string SeriesNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string VatPartner { get; set; }
    }
}
