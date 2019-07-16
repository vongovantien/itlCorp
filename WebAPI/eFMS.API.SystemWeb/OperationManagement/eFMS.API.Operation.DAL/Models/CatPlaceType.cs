﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.Service.Models
{
    public partial class CatPlaceType
    {
        public string Id { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
