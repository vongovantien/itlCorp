using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class HouseAirwayBillLastestReport
    {
        public string MAWB { get; set; }
        public string HWBNO { get; set; }
        public string ATTN { get; set; }
        public string ISSUED { get; set; }
        public string ConsigneeID { get; set; }
        public string Consignee { get; set; }
        public string ICASNC { get; set; }
        public string AccountingInfo { get; set; }
        public string AgentIATACode { get; set; }
        public string AccountNo { get; set; }
        public string DepartureAirport { get; set; }
        public string ReferrenceNo { get; set; }
        public string OSI { get; set; }
        public string FirstDestination { get; set; }
        public string FirstCarrier { get; set; }
        public string SecondDestination { get; set; }
        public string SecondCarrier { get; set; }
        public string ThirdDestination { get; set; }
        public string ThirdCarrier { get; set; }
        public string Currency { get; set; }
        public string CHGSCode { get; set; }
        public string WTPP { get; set; }
        public string WTCLL { get; set; }
        public string ORPP { get; set; }
        public string ORCLL { get; set; }
        public string DlvCarriage { get; set; }
        public string DlvCustoms { get; set; }
        public string LastDestination { get; set; }
        public string FlightNo { get; set; }
        public DateTime? FlightDate { get; set; }
        public string ConnectingFlight { get; set; }
        public DateTime? ConnectingFlightDate { get; set; }
        public string insurAmount { get; set; }
        public string HandlingInfo { get; set; }
        public string Notify { get; set; }
        public string SCI { get; set; }
        public string NoPieces { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal GrwDecimal { get; set; }
        public string Wlbs { get; set; }
        public string RateClass { get; set; }
        public string ItemNo { get; set; }
        public decimal? WChargeable { get; set; }
        public decimal ChWDecimal { get; set; }
        public string Rchge { get; set; }
        public string Ttal { get; set; }
        public string Description { get; set; }
        public string WghtPP { get; set; }
        public string WghtCC { get; set; }
        public string ValChPP { get; set; }
        public string ValChCC { get; set; }
        public string TxPP { get; set; }
        public string TxCC { get; set; }
        public string OrchW { get; set; }
        public string OChrVal { get; set; }
        public string TTChgAgntPP { get; set; }
        public string TTChgAgntCC { get; set; }
        public string TTCarrPP { get; set; }
        public string TTCarrCC { get; set; }
        public string TtalPP { get; set; }
        public string TtalCC { get; set; }
        public string CurConvRate { get; set; }
        public string CCChgDes { get; set; }
        public string SpecialNote { get; set; }
        public string ShipperCertf { get; set; }
        public string ExecutedOn { get; set; }
        public string ExecutedAt { get; set; }
        public string Signature { get; set; }
        public string Dimensions { get; set; }
        public byte[] ShipPicture { get; set; }
        public string PicMarks { get; set; }
        public string GoodsDelivery { get; set; }
        public string Airline { get; set; }
        public decimal? SeaAir { get; set; }
    }

    public class HouseAirwayBillLastestReportParams
    {
        public string MAWBN { get; set; }
    }
}
