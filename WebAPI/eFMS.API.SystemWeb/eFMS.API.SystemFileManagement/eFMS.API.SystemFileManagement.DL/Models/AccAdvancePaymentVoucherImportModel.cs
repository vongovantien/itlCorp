using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class AccAdvancePaymentVoucherImportModel : AcctAdvancePayment
    {
        public bool IsValid { get; set; }
        public string AdvanceNoError { get; set; }
        public string VoucherNoError { get; set; }
        public string VoucherDateError { get; set; }


    }
}
