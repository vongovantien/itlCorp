using eFMS.API.Common.Models;
using eFMS.API.Operation.Service.Models;
using System;

namespace eFMS.API.Operation.DL.Models
{
    public class CustomsDeclarationModel : CustomsDeclaration
    {
        public string CustomerName { get; set; }
        public string ImportCountryName { get; set; }
        public string ExportCountryName { get; set; }
        public string GatewayName { get; set; }
        public PermissionAllowBase Permission { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifieddName { get; set; }
        public bool? isDelete { get; set; }
        public Guid? jobId { get; set; }
    }

}
