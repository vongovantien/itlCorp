﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class SOAOPSModel
    {
        public string PartnerNameVN { get; set; }
        public string BillingAddressVN { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<ExportSOAOPS> exportSOAOPs { get; set; }
    }
}
