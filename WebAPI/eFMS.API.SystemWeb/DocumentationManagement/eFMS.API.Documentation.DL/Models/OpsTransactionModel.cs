using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class OpsTransactionModel: OpsTransaction
    {
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }

        public Nullable<int> SumCont { get; set; }
        public Nullable<decimal> SumCBM { get; set; }
        public Nullable<decimal> PackageQuantity { get; set; }
    }
}
