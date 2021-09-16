using System;
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
        public List<AdvanceRequestModel> Detail { get; set; }
    }

    public class RemoveVoucherAdvModel
    {
        public string VoucherNo { get; set; }
    }

    public class AdvanceRequestModel
    {
        public Guid RowID { get; set; }
        public string JobNo { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string ReferenceNo { get; set; }
    }
    
    public class VoucherExpense
    {
        public string DocID { get; set; }
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
