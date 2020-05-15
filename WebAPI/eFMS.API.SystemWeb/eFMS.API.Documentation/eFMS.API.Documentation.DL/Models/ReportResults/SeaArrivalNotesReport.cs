using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class SeaArrivalNotesReport
    {
        public string HWBNO { get; set; }
        public string ArrivalNo { get; set; }
        public string ReferrenceNo { get; set; }
        public string ISSUED { get; set; }
        public string ATTN { get; set; }
        public string Consignee { get; set; }
        public string Notify { get; set; }
        public string HandlingInfo { get; set; }
        public string ExecutedOn { get; set; }
        public string OceanVessel { get; set; }
        public string TotalPackages { get; set; }
        public string ShippingMarkImport { get; set; }
        public string OSI { get; set; }
        public DateTime FlightDate { get; set; }
        public DateTime DateConfirm { get; set; }
        public DateTime DatePackage { get; set; }
        public string LocalVessel { get; set; }
        public string ContSealNo { get; set; }
        public string ForCarrier { get; set; }
        public string DepartureAirport { get; set; }
        public string PortofDischarge { get; set; }
        public string PlaceDelivery { get; set; }
        public string ArrivalNote { get; set; }
        public string Description { get; set; }
        public string NoPieces { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal CBM { get; set; }
        public string Unit { get; set; }
        public bool blnShow { get; set; }
        public bool blnStick { get; set; }
        public bool blnRoot { get; set; }
        public string FreightCharges { get; set; }
        public decimal Qty { get; set; }
        public string QUnit { get; set; }
        public decimal TotalValue { get; set; }
        public string Curr { get; set; }
        public decimal VAT { get; set; }
        public string Notes { get; set; }
        public string ArrivalFooterNoitice { get; set; }
        public bool SeaFCL { get; set; }
        public string MaskNos { get; set; }
        public string DlvCustoms { get; set; }
        public string insurAmount { get; set; }
        public string BillType { get; set; }
        public DateTime DOPickup { get; set; }
        public decimal ExVND { get; set; }
        public string DecimalSymbol { get; set; }
        public string DigitSymbol { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
    }

    public class SeaArrivalNotesReportParams
    {
        public string No { get; set; }
        public string ShipperName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string MAWB { get; set; }
        public string Contact { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
    }
}
