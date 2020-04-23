namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class SeaFCLExportCargoManifest
    {
        public string TransID { get; set; }
        public string HBL { get; set; }
        public string Marks { get; set; }
        public string Nofpiece { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? SeaCBM { get; set; }
        public decimal NoOfAWB { get; set; }
        public string Destination { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string Descriptions { get; set; }
        public string FreightCharge { get; set; }
        public string Notify { get; set; }
        public string OnboardNote { get; set; }
        public string MaskNos { get; set; }
        public string TranShipmentTo { get; set; }
        public string BillType { get; set; }

        //public string ArrivalNo { get; set; }
        //public string ReferrenceNo { get; set; }
        //public string Marks { get; set; }
        //public string Nofpiece { get; set; }
        //public decimal? GW { get; set; }
        //public decimal? SeaCBM { get; set; }
        //public decimal NoOfAWB { get; set; }
        //public string Destination { get; set; }
        //public string Shipper { get; set; }
        //public string Consignee { get; set; }
        //public string Descriptions { get; set; }
        //public string FreightCharge { get; set; }
        //public string Notify { get; set; }
        //public string OnboardNote { get; set; }
        //public string MaskNos { get; set; }
        //public string TranShipmentTo { get; set; }
        //public string BillType { get; set; }
        //public decimal? NW { get; set; }
        //public string PortofDischarge { get; set; }
    }
    public class SeaCargoManifestParameter
    {
        public string ManifestNo { get; set; }
        public string Owner { get; set; }
        public string Marks { get; set; }
        public string Flight { get; set; }
        public string PortLading { get; set; }
        public string PortUnlading { get; set; }
        public string FlightDate { get; set; }
        public string Eta { get; set; }
        public string Consolidater { get; set; }
        public string DeConsolidater { get; set; }
        public string Forwarder { get; set; }
        public string OMB { get; set; }
        public string ContainerNo { get; set; }
        public string Agent { get; set; }
        public string QtyPacks { get; set; }
        public string TotalShipments { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
    }
}
