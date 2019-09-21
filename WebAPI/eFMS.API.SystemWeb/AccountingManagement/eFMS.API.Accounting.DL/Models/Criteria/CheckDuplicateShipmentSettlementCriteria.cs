using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
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
    }
}
