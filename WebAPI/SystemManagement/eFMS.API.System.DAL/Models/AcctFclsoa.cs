using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AcctFclsoa
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string CustomerId { get; set; }
        public bool? CustomerPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public bool? SentToCustomer { get; set; }
        public string SentByUser { get; set; }
        public DateTime? SentOn { get; set; }
        public decimal? Total { get; set; }
        public DateTime? ExportedDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid? BranchId { get; set; }

        public CatBranch Branch { get; set; }
    }
}
