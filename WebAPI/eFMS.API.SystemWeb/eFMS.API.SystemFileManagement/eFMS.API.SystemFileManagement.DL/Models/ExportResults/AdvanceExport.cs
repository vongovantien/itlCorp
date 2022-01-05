using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models.ExportResults
{
    public class AdvanceExport
    {
        public InfoAdvanceExport InfoAdvance { get; set; }
        public List<InfoShipmentAdvanceExport> ShipmentsAdvance { get; set; }
    }
}
