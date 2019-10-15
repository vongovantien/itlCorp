using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SysGroupRole
    {
        public short Id { get; set; }
        public short GroupId { get; set; }
        public short RoleId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual SysGroup Group { get; set; }
        public virtual SysRole Role { get; set; }
    }
}
