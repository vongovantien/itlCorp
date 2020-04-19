using eFMS.API.Operation.Service.Models;
using System;

namespace eFMS.API.Operation.DL.Models
{
    public class OpsStageAssignedModel: OpsStageAssigned
    {
        public string StageCode { get; set; }
        public string StageNameEN { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? DoneDate { get; set; }
    }
}
