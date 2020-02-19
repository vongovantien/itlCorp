using eFMS.API.Infrastructure.Models;
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

}
