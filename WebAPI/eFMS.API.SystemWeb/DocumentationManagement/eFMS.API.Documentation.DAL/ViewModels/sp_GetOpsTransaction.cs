using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetOpsTransaction
    {
        public Guid ID { get; set; }
        public string JobNo { get; set; }
        public string MBLNO { get; set; }
        public string HWBNO { get; set; }
        public Nullable<DateTime> ServiceDate { get; set; }
        public string ProductService { get; set; }
        public string ServiceMode { get; set; }
        public string ShipmentMode { get; set; }
        public string CustomerID { get; set; }
        public Nullable<Guid> POL { get; set; }
        public Nullable<Guid> POD { get; set; }
        public string SupplierID { get; set; }
        public string FlightVessel { get; set; }
        public string AgentID { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string BillingOpsID { get; set; }
        public Nullable<DateTime> FinishDate { get; set; }
        public Nullable<Guid> WarehouseID { get; set; }
        public string InvoiceNo { get; set; }
        public string FieldOpsID { get; set; }
        public Nullable<decimal> SumNetWeight { get; set; }
        public Nullable<decimal> SumGrossWeight { get; set; }
        public Nullable<decimal> SumChargeWeight { get; set; }
        public Nullable<decimal> SumCBM { get; set; }
        public Nullable<int> SumContainers { get; set; }
        public Nullable<int> SumPackages { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public string POLName { get; set; }
        public string PODName { get; set; }
        public string CurrentStatus { get; set; }
    }
}
