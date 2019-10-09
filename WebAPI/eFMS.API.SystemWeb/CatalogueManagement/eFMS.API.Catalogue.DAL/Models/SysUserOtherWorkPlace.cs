﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class SysUserOtherWorkPlace
    {
        public string UserId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? ActiveOn { get; set; }
        public Guid WorkPlaceId { get; set; }

        public virtual SysOffice WorkPlace { get; set; }
    }
}
