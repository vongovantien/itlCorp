﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class CatCommodity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string CommodityNameVn { get; set; }
        public string CommodityNameEn { get; set; }
        public short? CommodityGroupId { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
