using eFMS.API.Operation.Service.Models;
using System;

namespace eFMS.API.Operation.DL.Models
{
    public class OpsStageAssignedEditModel : OpsStageAssigned
    {
        public Guid? HblId { get; set; }
        public bool IsUseReplicate { get; set; }
    }
}
