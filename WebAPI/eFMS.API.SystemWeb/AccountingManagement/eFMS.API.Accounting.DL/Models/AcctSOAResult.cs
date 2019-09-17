using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctSOAResult : AcctSoa
    {
        public string PartnerName { get; set; }
        public int Shipment { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal TotalAmount { get { return this.DebitAmount - this.CreditAmount; } }
    }
}
