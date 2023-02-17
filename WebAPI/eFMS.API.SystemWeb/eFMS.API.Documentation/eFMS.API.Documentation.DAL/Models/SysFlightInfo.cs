using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysFlightInfo
    {
        public Guid Id { get; set; }
        public DateTime? PlanArrivalTime { get; set; }
        public DateTime? PlanDepartTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartTime { get; set; }
        public string ArrivalStation { get; set; }
        public string DepartStation { get; set; }
    }
}
