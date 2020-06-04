using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAccountingManagementResult : AccAccountingManagement
    {
        public string PartnerName { get; set; }
        public string CreatorName { get; set; }
    }
}
