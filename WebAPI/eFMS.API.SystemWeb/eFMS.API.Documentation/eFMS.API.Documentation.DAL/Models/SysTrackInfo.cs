using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysTrackInfo
    {
        public Guid Id { get; set; }
        public DateTime? PlanDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public string Event { get; set; }
        public string Station { get; set; }
        public string Status { get; set; }
        public string Piece { get; set; }
        public string Weight { get; set; }
    }
}
