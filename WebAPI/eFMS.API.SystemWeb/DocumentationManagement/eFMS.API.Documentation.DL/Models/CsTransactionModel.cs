using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.Service.Models;
using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsTransactionModel: CsTransaction
    {
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public string CreatorName { get; set; }
        public Nullable<int> SumCont { get; set; }
        public Nullable<decimal> SumCBM { get; set; }
    }
}
