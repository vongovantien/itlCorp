
namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class AgencyDebitCreditModel : CustomerDebitCreditModel
    {
        public string JobNo { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
    }
}
