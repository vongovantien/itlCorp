﻿using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeAddOrUpdateModel
    {
        public PermissionAllowBase Permission { get; set; }
        public CatCharge Charge { get; set; }
        public List<CatChargeDefaultAccount> ListChargeDefaultAccount { get; set; }
    }
}
