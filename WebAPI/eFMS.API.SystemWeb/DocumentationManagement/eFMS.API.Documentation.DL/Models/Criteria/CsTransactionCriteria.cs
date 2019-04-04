using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CsTransactionCriteria
    {
        public string All { get; set; }
        public string JobNo { get; set; }
        public string MAWB { get; set; }
        public string HWBNo { get; set; }
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string CustomerID { get; set; }
        public string NotifyPartyID { get; set; }
        public string SaleManID { get; set; }
        public string SealNo { get; set; }
        public string ContainerNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
