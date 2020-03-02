using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CsAirWayBill
    {
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
    }
}
