using eFMS.API.Operation.Service.Models;

namespace eFMS.API.Operation.DL.Models
{
    public class OpsStageAssignedEditModel : OpsStageAssigned
    {
        public bool IsUseReplicate { get; set; }
    }
}
