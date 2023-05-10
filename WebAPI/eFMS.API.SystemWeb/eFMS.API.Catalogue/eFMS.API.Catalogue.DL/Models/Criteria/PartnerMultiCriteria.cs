﻿using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models.Criteria
{
    public class PartnerMultiCriteria
    {
        public List<CatPartnerGroupEnum> PartnerGroups { get; set; }
        public bool? Active { get; set; }
        public string Service { get; set; }
        public string Office { get; set; }
        public string SalemanId { get; set; }
        public string ContractType { get; set; }
        public string PartnerType { get; set; }
        public bool? IsShowSaleman { get; set; }
    }
}
