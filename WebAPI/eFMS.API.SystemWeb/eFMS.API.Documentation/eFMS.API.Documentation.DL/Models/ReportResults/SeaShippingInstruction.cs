using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class SeaShippingInstruction
    {
        public string TRANSID { get; set; }
        public string ShippingMarkImport { get; set; }
        public string CheckAttachNull { get; set; }
        public DateTime? DatePackage { get; set; }
        public string ToPartner { get; set; }
        public string Attn { get; set; }
        public string Notify { get; set; }
        public string Re { get; set; }
        public string ShipperDf { get; set; }	
        public string GoodsDelivery { get; set; }
        public string RealShipper { get; set; }
        public string RealConsignee { get; set; }
        public string PortofLoading { get; set; }
        public string PortofDischarge { get; set; }	
        public string PlaceDelivery { get; set; }
        public string Vessel { get; set; }
        public string ContSealNo { get; set; }
        public string Etd { get; set; }
        public string ShippingMarks { get; set; }
        public string RateRequest { get; set; }
        public string Payment { get; set; }
        public string NoofPeace { get; set; }
        public string Containers { get; set; }
        public string MaskNos { get; set; }
        public string SIDescription { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? CBM { get; set; }
        public bool SeaLCL { get; set; }
        public bool SeaFCL { get; set; }
        public string NotitfyParty { get; set; }
        public string CTNS { get; set; }
        public string Measurement { get; set; }
        public string Qty { get; set; }	
    }
    public class SeaShippingInstructionParameter
    {
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public int DecimalNo { get; set; }
        public string TotalPackages { get; set; }
    }
}
