using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsBookingNote
    {
        public Guid Id { get; set; }
        public Guid? JobId { get; set; }
        public string From { get; set; }
        public string TelFrom { get; set; }
        public string To { get; set; }
        public string TelTo { get; set; }
        public string Revision { get; set; }
        public string BookingNo { get; set; }
        public DateTime? BookingDate { get; set; }
        public string ShipperId { get; set; }
        public string ShipperDescription { get; set; }
        public string ConsigneeId { get; set; }
        public string ConsigneeDescription { get; set; }
        public DateTime? DateOfStuffing { get; set; }
        public DateTime? ClosingTime { get; set; }
        public string PlaceOfStuffing { get; set; }
        public string Contact { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string Vessel { get; set; }
        public string Voy { get; set; }
        public string PaymentTerm { get; set; }
        public string FreightRate { get; set; }
        public string PlaceOfDelivery { get; set; }
        public string NoOfContainer { get; set; }
        public string Commodity { get; set; }
        public string SpecialRequest { get; set; }
        public decimal? Gw { get; set; }
        public decimal? Cbm { get; set; }
        public string ServiceRequired { get; set; }
        public string OtherTerms { get; set; }
        public string HblNo { get; set; }
        public string NoOfBl { get; set; }
        public string PickupAt { get; set; }
        public string DropoffAt { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public decimal? PackageQty { get; set; }
    }
}
