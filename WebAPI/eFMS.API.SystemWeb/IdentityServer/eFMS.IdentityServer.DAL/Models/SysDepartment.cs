using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.Service.Models
{
    public partial class SysDepartment
    {
        public short Id { get; set; }
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public Guid? BranchId { get; set; }
        public string ManagerId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
