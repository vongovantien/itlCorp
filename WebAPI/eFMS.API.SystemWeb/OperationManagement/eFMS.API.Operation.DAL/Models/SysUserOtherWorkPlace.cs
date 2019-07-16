﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.Service.Models
{
    public partial class SysUserOtherWorkPlace
    {
        public string UserId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public Guid WorkPlaceId { get; set; }

        public virtual SysUser User { get; set; }
        public virtual CatBranch WorkPlace { get; set; }
    }
}
