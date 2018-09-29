using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SaleSalesTarget
    {
        public int Id { get; set; }
        public string SalePersonId { get; set; }
        public string Type { get; set; }
        public decimal? Target { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string UserModified { get; set; }
        public DateTime DatetimeModified { get; set; }
    }
}
