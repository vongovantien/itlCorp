using eFMS.API.Operation.Service.ViewModels;
using System.Linq;

namespace eFMS.API.Operation.DL.Models
{
    public class OpsTransactionResult
    {
        public IQueryable<sp_GetStageViewList> OpsTransactions { get; set; }
        public int ToTalInProcessing { get; set; }
        public int ToTalFinish { get; set; }
        public int TotalOverdued { get; set; }
        public int TotalCanceled { get; set; }
    }
}
