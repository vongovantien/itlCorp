using eFMS.API.Documentation.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
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
