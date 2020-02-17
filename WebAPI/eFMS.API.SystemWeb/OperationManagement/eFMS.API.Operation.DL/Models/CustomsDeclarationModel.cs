using eFMS.API.Operation.Service.Models;

namespace eFMS.API.Operation.DL.Models
{
    public class CustomsDeclarationModel : CustomsDeclaration
    {
        public string CustomerName { get; set; }
        public string ImportCountryName { get; set; }
        public string ExportCountryName { get; set; }
        public string GatewayName { get; set; }
        public PermissionAllowBase Permission { get; set; }
    }

    public class PermissionAllowBase
    {
        public bool AllowUpdate { get; set; } = false;
        public bool AllowDelete { get; set; } = false;
        public bool AllowAddCharge { get; set; } = false;
        public bool AllowUpdateCharge { get; set; } = false;
        public bool AllowLock { get; set; } = false;
    }

}
