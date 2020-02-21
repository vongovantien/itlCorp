namespace eFMS.API.Common.Models
{
    public class PermissionAllowBase
    {
        public bool AllowUpdate { get; set; } = false;
        public bool AllowDelete { get; set; } = false;
        public bool AllowUpdateCharge { get; set; } = false;
        public bool AllowAssignStage { get; set; } = false;
        public bool AllowLock { get; set; } = false;
    }
}
