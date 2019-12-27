using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AirProofOfDeliveryReport
    {
        public string HWBNO { get; set; }
        public string ArrivalNo { get; set; }
        public string ReferrenceNo { get; set; }
        public string Issued { get; set; }
        public string ATTN { get; set; }
        public string Consignee { get; set; }
        public string Shipper { get; set; }
        public string Notify { get; set; }
        public string HandlingInfo { get; set; }
        public string ExecutedOn { get; set; }
        public string OceanVessel { get; set; }
        public string TotalPackages { get; set; }
        public string ShippingMarkImport { get; set; }
        public string Osi { get; set; }
        public DateTime FlightDate { get; set; }
        public DateTime DateConfirm { get; set; }
        public DateTime DatePackage { get; set; }
        public string LocalVessel { get; set; }
        public string ContSealNo { get; set; }
        public string ForCarrier { get; set; }
        public string DepartureAirport { get; set; }
        public string LastDestination { get; set; }
        public string PlaceDelivery { get; set; }
        public string ArrivalNote { get; set; }
        public string Description { get; set; }
        public int NoPieces { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal Cbm { get; set; }
        public string Unit { get; set; }
        public bool BlnShow { get; set; }
        public bool BlnStick { get; set; }
        public bool BlnRoot { get; set; }
        public string FreightCharges { get; set; }
        public decimal Qty { get; set; }
        public string QUnit { get; set; }
        public decimal TotalValue { get; set; }
        public string Curr { get; set; }
        public decimal VAT { get; set; }
        public string Notes { get; set; }
        public string ArrivalFooterNotice { get; set; }
        public bool SeaFCL { get; set; }
        public string MaskNos { get; set; }
        public string DlvCustoms { get; set; }
        public string InsurAmount { get; set; }
        public string MAWB { get; set; }
        public DateTime ExhDate { get; set; }
        public string ReceivedName { get; set; }
        public string ReceivedAddress { get; set; }
        public string ReceivedContactName { get; set; }
        public DateTime CussignedDate { get; set; }
        public string TimeAm { get; set; }
        public string SpecialInstruction { get; set; }
        public string BillType { get; set; }
        public string DecimalSymbol { get; set; }
        public string DigitSymbol { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
        public string PlaceOfReceipt { get; set; }
        public decimal NoofPieces{ get; set; }
        public string UnitPieaces { get; set; }
        public decimal GW { get; set; }
        public decimal NW { get; set; }
        public string SeaCBM { get; set; }
    }
    public class AirProofOfDeliveryReportParams
    {
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
    }
}
