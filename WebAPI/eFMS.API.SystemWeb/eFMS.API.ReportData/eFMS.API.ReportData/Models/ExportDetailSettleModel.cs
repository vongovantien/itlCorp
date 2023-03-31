using System;

namespace eFMS.API.ReportData.Models
{
    public class ExportDetailSettleModel
    {
        public Guid SettlementId { get; set; }
        public string Lang { get; set; }
        public string Action { get; set; }
        public string AccessToken { get; set; }
    }
}
