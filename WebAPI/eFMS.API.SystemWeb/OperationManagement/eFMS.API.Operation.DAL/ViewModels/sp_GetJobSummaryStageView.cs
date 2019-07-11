namespace eFMS.API.Operation.Service.ViewModels
{
    public class sp_GetJobSummaryStageView
    {
        public int Processing { get; set; }
        public int Done { get; set; }
        public int Overdued { get; set; }
        public int Canceled { get; set; }
    }
}
