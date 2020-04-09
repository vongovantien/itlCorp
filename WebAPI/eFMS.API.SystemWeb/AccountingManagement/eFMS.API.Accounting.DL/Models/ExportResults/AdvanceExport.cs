using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class AdvanceExport
    {
        public InfoAdvanceExport InfoAdvance { get; set; }
        public List<InfoShipmentAdvanceExport> ShipmentsAdvance { get; set; }
    }
}
