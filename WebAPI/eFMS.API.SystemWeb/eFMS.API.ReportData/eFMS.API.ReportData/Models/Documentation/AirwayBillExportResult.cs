using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models.Documentation
{
    public class AirwayBillExportResult
    {
        public string MawbNo1 { get; set; }
        public string MawbNo2 { get; set; }
        public string MawbNo3 { get; set; }
        public string AolCode { get; set; }
        public string AodCode { get; set; }
        public string HawbNo { get; set; }
        public string Shipper { set; get; }
        public string Consignee { get; set; }
        public string OfficeUserCurrent { get; set; }
        public string AirlineNameEn { get; set; }
        public string AirFrieghtDa { get; set; }
        public string DepartureAirport { get; set; }
        public string FirstTo { get; set; }
        public string FirstCarrier { get; set; }
        public string SecondTo { get; set; }
        public string SecondBy { get; set; }
        public string Currency { get; set; }
        public string Dclrca { get; set; }
        public string Dclrcus { get; set; }
        public string DestinationAirport { get; set; }
        public string FlightNo { get; set; }
        public DateTime? FlightDate { get; set; }
        public string IssuranceAmount { get; set; }
        public string HandingInfo { get; set; }
        public decimal? Pieces { get; set; }
        public decimal? Gw { get; set; }
        public decimal? Cw { get; set; }
        public decimal? RateCharge { get; set; }
        public string Total { get; set; }
        public string DesOfGood { get; set; }
        public string VolumeField { get; set; }
        public string PrepaidTotal { get; set; }
        public string CollectTotal { get; set; }
        public string IssueOn { get; set; }
        public DateTime? IssueDate { get; set; }
        public string PrepaidWt { get; set; }
        public string CollectWt { get; set; }
        public string PrepaidVal { get; set; }
        public string CollectVal { get; set; }
        public string PrepaidTax { get; set; }
        public string CollectTax { get; set; }
        public string PrepaidDueToCarrier { get; set; }
        public string CollectDueToCarrier { get; set; }
        public string Route { get; set; }
        public string PackageUnit { get; set; }
        public List<CsShipmentOtherChargeModel> OtherCharges { get; set; }
    }
}
