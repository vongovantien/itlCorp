
using System;

namespace eFMS.API.SystemFileManagement.DL.Models.Receipt
{
    public class AgencyDebitCreditModel : CustomerDebitCreditModel
    {
        public string JobNo { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
        public Guid? Hblid { get; set; }
    }
}
