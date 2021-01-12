using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CustomsDeclaration
    {
        public int Id { get; set; }
        public decimal? IdfromEcus { get; set; }
        public string JobNo { get; set; }
        public string ClearanceNo { get; set; }
        public string FirstClearanceNo { get; set; }
        public string PartnerTaxCode { get; set; }
        public DateTime? ClearanceDate { get; set; }
        public string Mblid { get; set; }
        public string Hblid { get; set; }
        public string PortCodeCk { get; set; }
        public string PortCodeNn { get; set; }
        public string UnitCode { get; set; }
        public int? QtyCont { get; set; }
        public string ServiceType { get; set; }
        public string Gateway { get; set; }
        public string Type { get; set; }
        public string Route { get; set; }
        public string DocumentType { get; set; }
        public string ExportCountryCode { get; set; }
        public string ImportCountryCode { get; set; }
        public string CommodityCode { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? Cbm { get; set; }
        public int? Pcs { get; set; }
        public string Source { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public DateTime? ConvertTime { get; set; }
        public string CargoType { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string AccountNo { get; set; }
    }
}
