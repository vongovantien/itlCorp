﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models.Receipt
{
    public class GroupShimentAgencyModel
    {
        public string JobNo { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
        public Guid? Hblid { get; set; }
        public decimal? UnpaidAmountVnd { get; set; }
        public decimal? UnpaidAmountUsd { get; set; }
        public List<AgencyDebitCreditModel> Invoices { get; set; }
    }
}
