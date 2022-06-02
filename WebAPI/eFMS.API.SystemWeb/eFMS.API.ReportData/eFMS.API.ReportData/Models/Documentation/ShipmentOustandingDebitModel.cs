using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Documentation
{
    public class ShipmentOustandingDebitModel
    {
        public string SaleManId { get; set; }
        public Guid? ContractId { get; set; }
        public string SalemanName { get; set; }
        public decimal? DebitAmount { get; set; }
        public string CreditCurrency { get; set; }
        public string JobNo { get; set; }
        public string HblNo { get; set; }
        public string OfficeName { get; set; }
        public string AccountNo { get; set; }
        public string CustomerName { get; set; }
        public string SalemanEmail { get; set; }
    }
}
