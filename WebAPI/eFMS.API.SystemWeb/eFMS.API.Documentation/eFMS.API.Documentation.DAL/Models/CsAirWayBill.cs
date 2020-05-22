using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsAirWayBill
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string Mblno1 { get; set; }
        public string Mblno2 { get; set; }
        public string Mblno3 { get; set; }
        public string ShipperId { get; set; }
        public string ShipperDescription { get; set; }
        public string ConsigneeId { get; set; }
        public string ConsigneeDescription { get; set; }
        public string ForwardingAgentId { get; set; }
        public string ForwardingAgentDescription { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public string PickupPlace { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string FirstCarrierBy { get; set; }
        public string FirstCarrierTo { get; set; }
        public string TransitPlaceTo1 { get; set; }
        public string TransitPlaceBy1 { get; set; }
        public string TransitPlaceTo2 { get; set; }
        public string TransitPlaceBy2 { get; set; }
        public string FlightNo { get; set; }
        public DateTime? FlightDate { get; set; }
        public string FreightPayment { get; set; }
        public string IssuranceAmount { get; set; }
        public string CurrencyId { get; set; }
        public string Chgs { get; set; }
        public string WtorValpayment { get; set; }
        public string OtherPayment { get; set; }
        public string Dclrca { get; set; }
        public string Dclrcus { get; set; }
        public string Route { get; set; }
        public Guid? WarehouseId { get; set; }
        public int? OriginBlnumber { get; set; }
        public string HandingInformation { get; set; }
        public string Notify { get; set; }
        public string IssuedPlace { get; set; }
        public DateTime? IssuedDate { get; set; }
        public int? PackageQty { get; set; }
        public decimal? GrossWeight { get; set; }
        public string Rclass { get; set; }
        public string ComItemNo { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? RateCharge { get; set; }
        public bool? Min { get; set; }
        public decimal? Total { get; set; }
        public string KgIb { get; set; }
        public int? SeaAir { get; set; }
        public decimal? Hw { get; set; }
        public decimal? Cbm { get; set; }
        public string VolumeField { get; set; }
        public string DesOfGoods { get; set; }
        public string OtherCharge { get; set; }
        public string Wtpp { get; set; }
        public string Valpp { get; set; }
        public string Taxpp { get; set; }
        public string DueAgentPp { get; set; }
        public string DueCarrierPp { get; set; }
        public string TotalPp { get; set; }
        public string Wtcll { get; set; }
        public string Valcll { get; set; }
        public string Taxcll { get; set; }
        public string DueAgentCll { get; set; }
        public string DueCarrierCll { get; set; }
        public string TotalCll { get; set; }
        public string ShippingMark { get; set; }
        public string IssuedBy { get; set; }
        public string CurrConvertRate { get; set; }
        public string Sci { get; set; }
        public string CcchargeInDrc { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
