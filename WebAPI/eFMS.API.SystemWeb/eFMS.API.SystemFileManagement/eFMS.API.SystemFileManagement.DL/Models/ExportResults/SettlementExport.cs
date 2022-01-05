using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models.ExportResults
{
    public class SettlementExport
    {
        public InfoSettlementExport InfoSettlement { get; set; }
        public List<InfoShipmentSettlementExport> ShipmentsSettlement { get; set; }
    }
}
