using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetShipmentDataWithOutstandingDebit
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
