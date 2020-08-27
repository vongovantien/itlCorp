using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class Shipments
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
        public string LockedLog { get; set; }
        public string CustomNo { get; set; }

    }
}
