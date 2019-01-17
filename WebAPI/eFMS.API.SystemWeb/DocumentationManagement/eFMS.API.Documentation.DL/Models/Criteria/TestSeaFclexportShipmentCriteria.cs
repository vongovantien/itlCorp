using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class TestSeaFclexportShipmentCriteria
    {
       public string SearchText { get; set; }
        public string ColoaderId { get; set; }
        public string AgentId { get; set; }
        public string NotifyPartyId { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
    }
}
