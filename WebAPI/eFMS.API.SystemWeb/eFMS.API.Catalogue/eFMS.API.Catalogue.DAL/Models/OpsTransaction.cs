using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class OpsTransaction
    {
        public Guid Id { get; set; }
        public Guid Hblid { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hwbno { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string ProductService { get; set; }
        public string ServiceMode { get; set; }
        public string ShipmentMode { get; set; }
        public string CustomerId { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string SupplierId { get; set; }
        public string FlightVessel { get; set; }
        public string AgentId { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string BillingOpsId { get; set; }
        public DateTime? FinishDate { get; set; }
        public Guid? WarehouseId { get; set; }
        public string InvoiceNo { get; set; }
        public string SalemanId { get; set; }
        public string FieldOpsId { get; set; }
        public Guid? ClearanceLocation { get; set; }
        public decimal? SumNetWeight { get; set; }
        public decimal? SumGrossWeight { get; set; }
        public decimal? SumChargeWeight { get; set; }
        public decimal? SumCbm { get; set; }
        public string ContainerDescription { get; set; }
        public int? SumContainers { get; set; }
        public int? SumPackages { get; set; }
        public short? PackageTypeId { get; set; }
        public string CurrentStatus { get; set; }
        public short? CommodityGroupId { get; set; }
        public string UnLockedLog { get; set; }
        public DateTime? LastDateUnLocked { get; set; }
        public DateTime? LockedDate { get; set; }
        public bool? IsLocked { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string LockedUser { get; set; }
        public string ShipmentType { get; set; }
        public string SalesGroupId { get; set; }
        public string SalesDepartmentId { get; set; }
        public string SalesOfficeId { get; set; }
        public string SalesCompanyId { get; set; }
        public string ServiceNo { get; set; }
        public Guid? ServiceHblId { get; set; }
    }
}
