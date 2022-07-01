using eFMS.API.ReportData.Models.Accounting;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public class DebitAmountDetail
    {
        public DebitAmountGeneralInfo DebitAmountGeneralInfo { get; set; }
        public List<DebitAmountDetailByContractModel> DebitAmountDetails { get; set; }
    }
}
