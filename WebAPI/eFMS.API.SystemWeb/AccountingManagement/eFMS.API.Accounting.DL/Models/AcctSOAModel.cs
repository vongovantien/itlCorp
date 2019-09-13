﻿using eFMS.API.Accounting.Service.Models;
using eFMS.API.Documentation.DL.Models;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctSoaModel : AcctSoa
    {
        public List<Surcharge> Surcharges { get; set; }
    }

}
