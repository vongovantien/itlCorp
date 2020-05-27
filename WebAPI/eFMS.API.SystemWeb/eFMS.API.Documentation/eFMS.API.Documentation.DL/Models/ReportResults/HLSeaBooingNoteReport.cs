using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class HLSeaBooingNoteReport
    {
        public string BookingID { get; set; }
        public string TransID { get; set; }
        public string LotNo { get; set; }
        public DateTime? DateMaking { get; set; }
        public string Revision { get; set; }
        public string Attn { get; set; }
        public string PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Homephone { get; set; }
        public string Workphone { get; set; }
        public string Fax { get; set; }
        public string Cell { get; set; }
        public string Taxcode { get; set; }
        public string ConsigneeCode { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsigneeAddress { get; set; }
        public string ReceiptAt { get; set; }
        public string Deliveryat { get; set; }
        public string ServiceMode { get; set; }
        public string SC { get; set; }
        public string PortofLading { get; set; }
        public string PortofUnlading { get; set; }
        public string ModeSea { get; set; }
        public string EstimatedVessel { get; set; }
        public DateTime? LoadingDate { get; set; }
        public DateTime? DestinationDate { get; set; }
        public string Quantity { get; set; }
        public string ContainerSize { get; set; }
        public string Commidity { get; set; }
        public decimal? GrosWeight { get; set; }
        public decimal? CBMSea { get; set; }
        public string SpecialRequest { get; set; }
        public string CloseTime20 { get; set; }
        public string CloseTime40 { get; set; }
        public string CloseTimeLCL { get; set; }
        public string PickupAt { get; set; }
        public string DropoffAt { get; set; }
        public string ContainerNo { get; set; }
        public string HBLData { get; set; }
        public string BlCorrection { get; set; }
        public string SCIACI { get; set; }
        public string Remark { get; set; }
    }

    public class HLSeaBooingNoteReportParameter
    {
        public string ContactList { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public decimal DecimalNo { get; set; }
        public string HBL { get; set; }
    }
}
