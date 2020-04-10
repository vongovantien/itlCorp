using System;

namespace eFMS.API.Operation.DL.Models.Criteria
{
    public class OpsTransactionCriteria
    {
        public bool IsSearchAll { get; set; }
        public bool IsSearchEdvance { get; set; }
        public string JobNo { get; set; }
        public string HwbNo { get; set; }
        public string ProductService { get; set; }
        public string ServiceMode { get; set; }
        public string ShipmentMode { get; set; }
        public string CustomerId { get; set; }
        public string FieldOps { get; set; }
        public DateTime? ServiceDateFrom { get; set; }
        public DateTime? ServiceDateTo { get; set; }

        public bool isLoadDefault { get; set; }
    }
}
