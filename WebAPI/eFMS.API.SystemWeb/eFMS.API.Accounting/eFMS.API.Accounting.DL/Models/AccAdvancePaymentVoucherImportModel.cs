using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAdvancePaymentVoucherImportModel : AcctAdvancePayment
    {
        public bool IsValid { get; set; }
        public string AdvanceNoError { get; set; }
        public string VoucherNoError { get; set; }
        public string VoucherDateError { get; set; }
        public bool ValidVoucherDate { get; set; }
        public string VoucherDateStr { get; set; }


    }
}
