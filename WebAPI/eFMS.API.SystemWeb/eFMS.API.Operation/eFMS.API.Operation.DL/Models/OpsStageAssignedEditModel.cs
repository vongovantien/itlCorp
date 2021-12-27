using System;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Operation.DL.Models
{
    public class OpsStageAssignedEditModel //: OpsStageAssigned
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }

        [Required]
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
        public bool IsUseReplicate { get; set; }
    }
}
