namespace eFMS.API.Infrastructure.Models
{
    public class PermissionAllowBase
    {
        public bool AllowUpdate { get; set; } = false;
        public bool AllowDelete { get; set; } = false;
        public bool AllowAddCharge { get; set; } = false;
        public bool AllowUpdateCharge { get; set; } = false;
        public bool AllowLock { get; set; } = false;
    }
}
