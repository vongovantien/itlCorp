using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class SeaHBillofLadingReport
    {
        public string HWBNO { get; set; }
        public string OSI { get; set; }
        public string CheckNullAttach { get; set; }
        public string ReferrenceNo { get; set; }
        public string Shipper { get; set; }
        public string ConsigneeID { get; set; }
        public string Consignee { get; set; }
        public string Notify { get; set; }
        public string PlaceAtReceipt { get; set; }
        public string PlaceDelivery { get; set; }
        public string LocalVessel { get; set; }
        public string FromSea { get; set; }
        public string OceanVessel { get; set; }
        public string DepartureAirport { get; set; }
        public string PortofDischarge { get; set; }
        public string TranShipmentTo { get; set; }
        public string GoodsDelivery { get; set; }
        public string CleanOnBoard { get; set; }
        public string MaskNos { get; set; }
        public string NoPieces { get; set; }
        public string Qty { get; set; }
        public string Description { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal GrwDecimal { get; set; }
        public string Unit { get; set; }
        public decimal CBM { get; set; }
        public decimal CBMDecimal { get; set; }
        public string SpecialNote { get; set; }
        public string TotalPackages { get; set; }
        public string OriginCode { get; set; }
        public string ICASNC { get; set; }
        public string Movement { get; set; }
        public string AccountingInfo { get; set; }
        public string SayWord { get; set; }
        public string strOriginLandPP { get; set; }
        public string strOriginLandCC { get; set; }
        public string strOriginTHCPP { get; set; }
        public string strOriginTHCCC { get; set; }
        public string strSeafreightPP { get; set; }
        public string strSeafreightCC { get; set; }
        public string strDesTHCPP { get; set; }
        public string strDesTHCCC { get; set; }
        public string strDesLandPP { get; set; }
        public string strDesLandCC { get; set; }
        public string FreightPayAt { get; set; }
        public string ExecutedAt { get; set; }
        public string ExecutedOn { get; set; }
        public string NoofOriginBL { get; set; }
        public string ForCarrier { get; set; }
        public bool SeaLCL { get; set; }
        public bool SeaFCL { get; set; }
        public string ExportReferences { get; set; }
        public string AlsoNotify { get; set; }
        public string Signature { get; set; }
        public DateTime SailingDate { get; set; }
        public byte[] ShipPicture { get; set; }
        public string PicMarks { get; set; }
        public string ContSTC { get; set; }
        public string SpecialText { get; set; }
        public string Service { get; set; }
        public string AttachList { get; set; }
    }

    public class SeaHBillofLadingReportParams1
    {
        public string Packages { get; set; }
        public string GrossWeight { get; set; }
        public string Measurement { get; set; }
    }

    public class SeaHBillofLadingReportITLFRAMEParams
    {
        public string Packages { get; set; }
        public string GrossWeight { get; set; }
        public string Measurement { get; set; }
        public string TextInfo { get; set; }
    }

    public class SeaHBillofLadingReportParams2 : SeaHBillofLadingReportParams1
    {
        public string DocumentNo { get; set; }
    }
}
