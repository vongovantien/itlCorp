using System.Collections.Generic;

namespace eFMS.API.ReportData.Models.Accounting
{
    public class AdvanceExport
    {
        public InfoAdvanceExport InfoAdvance { get; set; }
        public List<InfoShipmentAdvanceExport> ShipmentsAdvance { get; set; }
    }
}
