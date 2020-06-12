using eFMS.API.System.Service.Models;

namespace eFMS.API.System.DL.Models
{
    public class SysAuthorizedApprovalModel : SysAuthorizedApproval
    {
        public string AuthorizerName { get; set; }
        public string CommissionerName { get; set; }
    }
}
