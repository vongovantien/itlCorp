using System;

namespace eFMS.API.Operation.DL.Models
{
    public class OpsTransactionModel
    {
        public Guid ID { get; set; }
        public string JobNo { get; set; }
        public Guid HBLID { get; set; }
        public string CustomerID { get; set; }
        public string PartnerName_EN { get; set; }
        public string FieldOpsID { get; set; }
        public string UserName { get; set; }

        public string JsonStages { get; set; }
        public int Progress { get; set; }
    }
}
