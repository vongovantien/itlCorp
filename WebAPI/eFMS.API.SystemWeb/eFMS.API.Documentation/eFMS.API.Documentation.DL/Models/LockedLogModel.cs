using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Models
{
    public class LockedLogModel
    {
        public Guid Id { get; set; }
        public string AdvanceNo { get; set; }
        public string SettlementNo { get; set; }
        public string UnLockedLog { get; set; }
        public string OPSShipmentNo { get; set; }
        public string CSShipmentNo { get; set; }
        public bool? IsLocked { get; set; }
    }
    public class LockedLogResultModel
    {
        public IQueryable<LockedLogModel> LockedLogs { get; set; }
        public List<string> Logs { get; set; }
    }
}
