﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class CatPartnerGroup
    {
        public string Id { get; set; }
        public string GroupNameVn { get; set; }
        public string GroupNameEn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
