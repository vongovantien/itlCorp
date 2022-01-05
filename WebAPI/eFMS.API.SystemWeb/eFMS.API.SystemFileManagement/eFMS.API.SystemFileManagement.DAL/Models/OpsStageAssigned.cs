using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.Service.Models
{
    public partial class OpsStageAssigned
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public int StageId { get; set; }
        public string Name { get; set; }
        public int? OrderNumberProcessed { get; set; }
        public string MainPersonInCharge { get; set; }
        public string RealPersonInCharge { get; set; }
        public decimal? ProcessTime { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
