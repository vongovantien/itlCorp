using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class OpsTransaction
    {
        public Guid Id { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hblno { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string ProductService { get; set; }
        public string ServiceMode { get; set; }
        public string ShipmentMode { get; set; }
        public string CustomerId { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string SupplierId { get; set; }
        public string FlightVessel { get; set; }
        public string Agent { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string BillingOps { get; set; }
        public DateTime? FinishDate { get; set; }
        public Guid? WarehouseId { get; set; }
        public string InvoiceNo { get; set; }
        public string Saleman { get; set; }
        public string FieldOps { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? Cbm { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
