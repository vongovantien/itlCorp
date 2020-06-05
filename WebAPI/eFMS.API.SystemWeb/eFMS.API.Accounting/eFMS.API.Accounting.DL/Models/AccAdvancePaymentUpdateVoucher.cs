using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAdvancePaymentUpdateVoucher
    {
        public string AdvanceNo { get; set; }
        public string VoucherNo { get; set; }
        public Guid Id { get; set; }
    }
}
