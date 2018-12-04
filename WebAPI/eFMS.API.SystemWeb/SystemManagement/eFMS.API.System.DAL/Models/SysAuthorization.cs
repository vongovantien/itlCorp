using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysAuthorization
    {
        public SysAuthorization()
        {
            SysAuthorizationDetail = new HashSet<SysAuthorizationDetail>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public string AssignTo { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public SysUser AssignToNavigation { get; set; }
        public SysUser User { get; set; }
        public ICollection<SysAuthorizationDetail> SysAuthorizationDetail { get; set; }
    }
}
