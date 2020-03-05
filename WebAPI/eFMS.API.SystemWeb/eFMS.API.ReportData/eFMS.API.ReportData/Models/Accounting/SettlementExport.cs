using System.Collections.Generic;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class SettlementExport
    {
        public InfoSettlementExport InfoSettlement { get; set; }
        public List<InfoShipmentSettlementExport> ShipmentsSettlement { get; set; }
    }
}
