﻿using eFMS.API.Accounting.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctSoaModel : AcctSoa
    {
        public List<Surcharge> Surcharges { get; set; }
        public bool IsUseCommissionExRate { get; set; }
    }

    public class RejectSoaModel
    {
        public string Id { get; set; }
        public string Reason { get; set; }
    }
}
