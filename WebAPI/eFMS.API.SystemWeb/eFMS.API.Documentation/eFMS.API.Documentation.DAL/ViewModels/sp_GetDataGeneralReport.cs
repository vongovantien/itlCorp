using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetDataGeneralReport
    {
        public string JobNo { get; set; }
        public string Mawb { get; set; }
        public Guid? HblId { get; set; }
        public string HwbNo { get; set; }
        public string CustomerId { get; set; }
        public string ColoaderId { get; set; }
        public string AgentId { get; set; }
        public int? PackageQty { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string PersonInCharge { get; set; }
        public string SalemanId { get; set; }
        public string TransactionType { get; set; }
        public decimal? ChargeWeight { get; set; }
    }
}
