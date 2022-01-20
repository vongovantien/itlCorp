﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class CombineOPSModel
    {
        public string PartnerNameVN { get; set; }
        public string BillingAddressVN { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string No { get; set; }
        public bool IsDisplayLogo { get; set; }
        public List<ExportCombineOPS> exportOPS { get; set; }
    }
}
