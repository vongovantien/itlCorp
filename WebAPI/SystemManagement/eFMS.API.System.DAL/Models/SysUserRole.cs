﻿using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysUserRole
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public short RoleId { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public SysRole Role { get; set; }
        public SysUser User { get; set; }
        public CatPlace WorkPlace { get; set; }
    }
}
