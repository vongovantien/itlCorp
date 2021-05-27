﻿using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Models
{
    public class VoucherAdvance
    {
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal? PaymentTerm { get; set; }
        public string AdvanceNo { get; set; }
        public Guid AdvanceID { get; set; }
    }

    public class RemoveVoucherAdvModel
    {
        public string VoucherNo { get; set; }
    }

    public class VoucherExpense
    {
        public Guid DocID { get; set; }
        public string DocNO { get; set; }
        public string DocType { get; set; }
        public List<VoucherExpenseCharge> Detail { get; set; }
    }

    public class VoucherExpenseCharge
    {
        public Guid RowID { get; set; }
        public string JobNo { get; set; }
        public string VoucherNO { get; set; }
        public DateTime? VoucherDate { get; set; }
    }
}
