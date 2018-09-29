using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AcctSoahistory
    {
        public int Id { get; set; }
        public string Soano { get; set; }
        public DateTime? CustomerConfirmDate { get; set; }
        public decimal? PaidPrice { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public decimal? FreightPrice { get; set; }
        public decimal? BehalfPrice { get; set; }
        public decimal? PaidFreightPrice { get; set; }
        public decimal? PaidBehalfPrice { get; set; }
        public bool? CustomerPaid { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}
