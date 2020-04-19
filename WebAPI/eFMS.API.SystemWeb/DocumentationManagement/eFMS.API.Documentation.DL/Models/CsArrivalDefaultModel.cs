using eFMS.API.Documentation.DL.Common;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsArrivalDefaultModel
    {
        public TransactionTypeEnum Type { get; set; }
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public string ArrivalHeader { get; set; }
        public string ArrivalFooter { get; set; }
    }
}
