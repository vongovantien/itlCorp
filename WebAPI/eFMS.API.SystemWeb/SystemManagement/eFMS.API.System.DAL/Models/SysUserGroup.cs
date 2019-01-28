using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysUserGroup
    {
        public SysUserGroup()
        {
            SysUser = new HashSet<SysUser>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual ICollection<SysUser> SysUser { get; set; }
    }
}
