using eFMS.API.Documentation.Service.Models;
namespace eFMS.API.Documentation.DL.Models
{
    public class CsWorkOrderModel: CsWorkOrder
    {
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string PartnerName { get; set; }
    }

    public class CsWorkOrderViewModel
    {
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string PartnerName { get; set; }
        public string SalesmanName { get; set; }
        public string PodCode { get; set; }
        public string PolCode { get; set; }
        public string ApprovedStatus { get; set; }
        public string Code { get; set; }
        public string DatetimeCreated { get; set; }
        public string DatetimeModified { get; set; }
        public string Service { get; set; }
    }
}
