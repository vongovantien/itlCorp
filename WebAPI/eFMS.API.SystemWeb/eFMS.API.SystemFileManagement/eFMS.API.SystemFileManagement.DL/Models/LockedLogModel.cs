using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class LockedLogModel
    {
        public Guid Id { get; set; }
        public string AdvanceNo { get; set; }
        public string SettlementNo { get; set; }
        public string LockedLog { get; set; }
        public string OPSShipmentNo { get; set; }
        public string CSShipmentNo { get; set; }
    }
    public class LockedLogResultModel
    {
        public IQueryable<LockedLogModel> LockedLogs { get; set; }
        public List<string> Logs { get; set; }
    }
}
