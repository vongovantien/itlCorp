using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctSOAResult : AcctSoa
    {
        public string HBL { get; set; }
        public string PartnerName { get; set; }
        public int? Shipment { get; set; }
        public decimal? TotalAmount { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
    }
}
