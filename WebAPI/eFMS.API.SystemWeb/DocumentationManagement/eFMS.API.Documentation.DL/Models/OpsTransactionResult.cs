using System.Linq;

namespace eFMS.API.Documentation.DL.Models
{
    public class OpsTransactionResult
    {
        public IQueryable<OpsTransactionModel> OpsTransactions { get; set; }
        public int ToTalInProcessing { get; set; }
        public int ToTalFinish { get; set; }
        public int TotalOverdued { get; set; }
        public int TotalCanceled { get; set; }
    }
}
