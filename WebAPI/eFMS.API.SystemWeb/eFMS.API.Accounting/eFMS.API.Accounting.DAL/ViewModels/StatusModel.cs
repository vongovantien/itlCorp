namespace eFMS.API.Accounting.Service.ViewModels
{
    public class StatusModel
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public decimal TotalRowAffected { get; set; }
    }
}
