﻿using System;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatCommodityGroupViewModel
    {
        public short Id { get; set; }
        public string GroupName { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InActiveOn { get; set; }
    }
}
