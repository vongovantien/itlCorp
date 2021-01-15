using eFMS.API.System.Service.Models;

namespace eFMS.API.System.DL.Models
{
    public class SysAuthorizedApprovalModel : SysAuthorizedApproval
    {
        public string AuthorizerName { get; set; }
        public string CommissionerName { get; set; }
        public string NameUserCreated { get; set; }
        public string NameUserModified { get; set; }
        public string OfficeCommissionerName { get; set; }
    }
}
