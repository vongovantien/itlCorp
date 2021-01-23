using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class CheckDuplicateShipmentSettlementCriteria
    {
        public Guid SurchargeID { get; set; }
        public Guid ChargeID { get; set; }
        public string TypeCharge { get; set; }
        public Guid HBLID { get; set; }
        public string Partner { get; set; }
        public string CustomNo { get; set; }
        public string InvoiceNo { get; set; }
        public string ContNo { get; set; }
        public string JobNo { get; set; }
        public string HBLNo { get; set; }
        public string MBLNo { get; set; }
        public string Notes { get; set; }
    }

    public class DuplicateShipmentSettlementResultModel
    {
        public string JobNo { get; set; }
        public string MBLNo { get; set; }
        public string HBLNo{ get; set; }
        public Guid ChargeId { get; set; }
    }
}
