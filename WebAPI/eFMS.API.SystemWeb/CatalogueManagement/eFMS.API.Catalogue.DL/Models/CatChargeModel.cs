using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChargeModel : CatCharge
    {
        public string currency { get; set; }
        public string unit { get; set; }

        public PermissionAllowBase Permission { get; set; }
    }
}
