using eFMS.API.Documentation.DL.Common;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class ShipmentCriteria
    {
        public ShipmentPropertySearch ShipmentPropertySearch { get; set; }
        public List<string> Keywords { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<string> OfficeIds { get; set; }
    }
    public enum ShipmentPropertySearch
    {
        JOBID = 1,
        MBL = 2,
        HBL = 3,
        CD = 4
    }
}
