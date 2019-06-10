using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class CustomsDeclaration
    {
        public decimal Id { get; set; }
        public string ClearanceNo { get; set; }
        public string FirstClearanceNo { get; set; }
        public string CustomerId { get; set; }
        public string PartnerTaxCode { get; set; }
        public DateTime? ClearanceDate { get; set; }
        public string Mblid { get; set; }
        public string Hblid { get; set; }
        public string PortCodeCk { get; set; }
        public string PortCodeNn { get; set; }
        public string UnitId { get; set; }
        public int? QtyCont { get; set; }
        public string ServiceType { get; set; }
        public string Gateway { get; set; }
        public string Type { get; set; }
        public string Route { get; set; }
        public string DocumentType { get; set; }
        public Guid? ExportCountryId { get; set; }
        public Guid? ImportcountryId { get; set; }
        public int? CommodityId { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? Cbm { get; set; }
        public int? ContQuantity { get; set; }
        public string Pcs { get; set; }
        public string Source { get; set; }
        public string Note { get; set; }
    }
}
