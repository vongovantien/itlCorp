using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysStatus
    {
        public SysStatus()
        {
            CatOrderStatusReason = new HashSet<CatOrderStatusReason>();
        }

        public short Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CatOrderStatusReason> CatOrderStatusReason { get; set; }
    }
}
