﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.DL.Models
{
    public class VoucherAdvance
    {
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal PaymnetTerm { get; set; }
        public string AdvanceNo { get; set; }
        public string AdvanceID { get; set; }
    }
}
