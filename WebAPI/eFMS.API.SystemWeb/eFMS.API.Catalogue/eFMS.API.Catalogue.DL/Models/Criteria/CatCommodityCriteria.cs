﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class CatCommodityCriteria
    {
        public string All { get; set; }
        public string CommodityNameVn { get; set; }
        public string CommodityNameEn { get; set; }
        public short? CommodityGroupId { get; set; }
        public string CommodityGroupNameVn { get; set; }
        public string CommodityGroupNameEn { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InActiveOn { get; set; }
    }
}
