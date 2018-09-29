using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysSynchronizationError
    {
        public Guid Id { get; set; }
        public Guid? BranchId { get; set; }
        public string TableCrm { get; set; }
        public string SugarId { get; set; }
        public string OwnId { get; set; }
        public string ErrorDescription { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
