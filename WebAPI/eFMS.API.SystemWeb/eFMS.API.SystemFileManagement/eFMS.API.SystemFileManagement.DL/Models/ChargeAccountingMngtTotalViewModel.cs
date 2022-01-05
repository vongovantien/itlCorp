using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class ChargeAccountingMngtTotalViewModel
    {
        public List<ChargeOfAccountingManagementModel> Charges { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAmountVat { get; set; }
        public decimal? TotalAmount { get; set; } //TotalAmountVND + TotalAmountVatVND
    }
}
