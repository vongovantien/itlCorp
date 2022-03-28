using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetShipmentAssignPIC
    {
        public Guid Id { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomerId { get; set; }
        public string AgentId { get; set; }
        public string CarrierId { get; set; }
        public Guid HBLID { get; set; }
        public string Service { get; set; }
    }
}
