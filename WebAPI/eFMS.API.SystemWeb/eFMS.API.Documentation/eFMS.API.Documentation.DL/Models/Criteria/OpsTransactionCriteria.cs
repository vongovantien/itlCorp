using eFMS.API.Common.Globals;
using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class OpsTransactionCriteria
    {
        public string All { get; set; }
        public string JobNo { get; set; }
        public string Hwbno { get; set; }
        public string Mblno { get; set; }
        public string ClearanceNo { get; set; }
        public string CreditDebitInvoice { get; set; }
        public string ProductService { get; set; }
        public string ServiceMode { get; set; }
        public string ShipmentMode { get; set; }
        public string CustomerId { get; set; }
        public string FieldOps { get; set; }
        public DateTime? ServiceDateFrom { get; set; }
        public DateTime? ServiceDateTo { get; set; }
        public PermissionRange RangeSearch { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
    }
}
